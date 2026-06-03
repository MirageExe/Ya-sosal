using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Popups;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Verbs;
using Content.Shared._Rat.Silicons.Borgs;
using Robust.Shared.Player;

namespace Content.Server._Rat.Silicons.Borgs;

public sealed class BorgAiCommandSystem : EntitySystem
{
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly BorgAiToolSystem _tools = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Uncoupled subscription: ConstructionComponent already uses a directed GetVerbsEvent handler.
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var target = args.Target;

        if (HasComp<BorgAiCommandComponent>(target) && HasComp<BorgChassisComponent>(target))
        {
            AddOrderVerb(args, target, BorgAiOrderType.Idle, "borg-ai-verb-idle");
            AddOrderVerb(args, target, BorgAiOrderType.Gather, "borg-ai-verb-gather");
        }

        if (BorgAiValidation.CanDisassemble(target, EntityManager))
            AddTargetOrderVerb(args, target, BorgAiOrderType.Disassemble, "borg-ai-verb-disassemble");

        if (!BorgAiValidation.CanHeal(target, _mobState, EntityManager))
            return;

        AddTargetOrderVerb(args, target, BorgAiOrderType.Heal, "borg-ai-verb-heal");
        AddTargetOrderVerb(args, target, BorgAiOrderType.Defend, "borg-ai-verb-defend");
        AddTargetOrderVerb(args, target, BorgAiOrderType.Solve, "borg-ai-verb-solve");
    }

    private void AddOrderVerb(GetVerbsEvent<Verb> args, EntityUid borg, BorgAiOrderType order, string loc)
    {
        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString(loc),
            Act = () => TryIssueOrder(borg, order, null, args.User),
        });
    }

    private void AddTargetOrderVerb(GetVerbsEvent<Verb> args, EntityUid target, BorgAiOrderType order, string loc)
    {
        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString(loc),
            Act = () =>
            {
                if (!TryFindCommandableBorg(args.User, out var borg))
                {
                    _popup.PopupEntity(Loc.GetString("borg-ai-no-borg"), args.User, args.User);
                    return;
                }

                TryIssueOrder(borg, order, target, args.User);
            },
        });
    }

    public bool TryIssueOrder(EntityUid borg, BorgAiOrderType order, EntityUid? target, EntityUid? issuer = null, string? targetName = null)
    {
        if (!TryComp<BorgAiCommandComponent>(borg, out var cmd) || !TryComp<HTNComponent>(borg, out var htn))
            return false;

        EntityUid? resolvedTarget = target;

        if ((!resolvedTarget.HasValue || !resolvedTarget.Value.IsValid()) && !string.IsNullOrEmpty(targetName))
            resolvedTarget = ResolveTargetByName(borg, targetName, cmd.CommandRange);

        if (resolvedTarget is { } t && !ValidateOrderTarget(borg, order, t, issuer, out var fail))
        {
            if (issuer != null && fail != null)
                _popup.PopupEntity(Loc.GetString(fail), borg, issuer.Value);
            return false;
        }

        cmd.CurrentOrder = order;
        cmd.OrderTarget = resolvedTarget;
        Dirty(borg, cmd);

        _npc.SetBlackboard(borg, BorgAiBlackboard.Order, order);

        if (resolvedTarget is { } validTarget)
        {
            _npc.SetBlackboard(borg, BorgAiBlackboard.Target, validTarget);
            _npc.SetBlackboard(borg, NPCBlackboard.CurrentOrderedTarget, validTarget);
            _npc.SetBlackboard(borg, "TargetCoordinates", Transform(validTarget).Coordinates);
        }
        else
        {
            blackboardRemove(borg, BorgAiBlackboard.Target);
            blackboardRemove(borg, NPCBlackboard.CurrentOrderedTarget);
        }

        _tools.PrepareForWork(borg);

        _htn.ShutdownPlan(htn);
        htn.PlanAccumulator = 0f;
        _htn.Replan(htn);

        if (issuer != null)
            _popup.PopupEntity(Loc.GetString("borg-ai-order-issued", ("order", Loc.GetString($"borg-ai-order-{order.ToString().ToLower()}"))), borg, issuer.Value);

        return true;
    }

    private void blackboardRemove(EntityUid borg, string key)
    {
        if (TryComp<HTNComponent>(borg, out var htn))
            htn.Blackboard.Remove<EntityUid>(key);
    }

    private bool ValidateOrderTarget(EntityUid borg, BorgAiOrderType order, EntityUid target, EntityUid? issuer, out string? fail)
    {
        fail = null;
        var tools = EntityManager.System<BorgAiToolSystem>();

        switch (order)
        {
            case BorgAiOrderType.Disassemble:
                if (!BorgAiValidation.CanDisassemble(target, EntityManager))
                    fail = "borg-ai-fail-not-deconstructable";
                break;
            case BorgAiOrderType.Heal:
                if (!BorgAiValidation.CanHeal(target, _mobState, EntityManager))
                    fail = "borg-ai-fail-not-healable";
                else if (!BorgAiValidation.NeedsHealing(target, EntityManager))
                    fail = "borg-ai-fail-already-healthy";
                break;
            case BorgAiOrderType.Defend:
            case BorgAiOrderType.Solve:
            case BorgAiOrderType.Gather:
                break;
            case BorgAiOrderType.Idle:
                return true;
        }

        if (fail != null)
            return false;

        if (!tools.IsInInteractionRange(borg, target) && order != BorgAiOrderType.Defend)
        {
            // Allow out-of-range: cyborg will pathfind.
            return true;
        }

        return true;
    }

    public bool TryFindCommandableBorg(EntityUid user, out EntityUid borg)
    {
        borg = default;
        var range = BorgAiBlackboard.DefaultCommandRange;
        var userPos = Transform(user).Coordinates;

        foreach (var ent in _lookup.GetEntitiesInRange(userPos, range))
        {
            if (!HasComp<BorgAiCommandComponent>(ent) || !HasComp<BorgChassisComponent>(ent))
                continue;

            borg = ent;
            return true;
        }

        return false;
    }

    public EntityUid? ResolveTargetByName(EntityUid borg, string name, float range)
    {
        var coords = Transform(borg).Coordinates;
        var query = EntityQueryEnumerator<MetaDataComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var meta, out var xform))
        {
            if (!coords.InRange(EntityManager, xform.Coordinates, range))
                continue;

            if (!Identity.Name(uid, EntityManager).Contains(name, StringComparison.OrdinalIgnoreCase)
                && !meta.EntityName.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return uid;
        }

        return null;
    }
}
