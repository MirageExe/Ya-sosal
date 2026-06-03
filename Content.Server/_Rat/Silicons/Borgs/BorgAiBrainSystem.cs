using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Server.NPC.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Silicons.Borgs;
using Content.Shared.Interaction;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared._Rat.Silicons.Borgs;
using Robust.Shared.Containers;

namespace Content.Server._Rat.Silicons.Borgs;

/// <summary>
/// Activates HTN AI when a <see cref="BorgAiBrainComponent"/> is installed in a cyborg.
/// </summary>
public sealed class BorgAiBrainSystem : EntitySystem
{
    [Dependency] private readonly BorgAiToolSystem _tools = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly NPCSystem _npc = default!;

    private const string RootTask = "BorgAiCompound";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgAiBrainComponent, EntGotInsertedIntoContainerMessage>(OnBrainInserted);
        SubscribeLocalEvent<BorgAiBrainComponent, EntGotRemovedFromContainerMessage>(OnBrainRemoved);
        SubscribeLocalEvent<BorgAiBrainComponent, ComponentStartup>(OnAiBrainStartup);
        SubscribeLocalEvent<HTNComponent, ComponentStartup>(OnHtnStartup);
    }

    private void OnAiBrainStartup(EntityUid uid, BorgAiBrainComponent component, ComponentStartup args)
    {
        if (!HasComp<BorgBrainComponent>(uid))
            return;

        RemComp<ToggleableGhostRoleComponent>(uid);
    }

    private void OnHtnStartup(EntityUid uid, HTNComponent component, ComponentStartup args)
    {
        if (!HasComp<BorgAiCommandComponent>(uid))
            return;

        InitializeHtn(uid, component);
    }

    private void OnBrainInserted(EntityUid uid, BorgAiBrainComponent component, EntGotInsertedIntoContainerMessage args)
    {
        var chassis = args.Container.Owner;

        if (!TryComp<BorgChassisComponent>(chassis, out var chassisComp)
            || args.Container.ID != chassisComp.BrainContainerId)
        {
            return;
        }

        EnableAi(chassis);
    }

    private void OnBrainRemoved(EntityUid uid, BorgAiBrainComponent component, EntGotRemovedFromContainerMessage args)
    {
        var chassis = args.Container.Owner;

        if (!TryComp<BorgChassisComponent>(chassis, out var chassisComp)
            || args.Container.ID != chassisComp.BrainContainerId)
        {
            return;
        }

        DisableAi(chassis);
    }

    public void OnChassisMindAdded(EntityUid chassis)
    {
        if (_mind.TryGetMind(chassis, out _, out var mind) && mind.Session != null)
            DisableAi(chassis);
    }

    public void OnChassisMindRemoved(EntityUid chassis, BorgChassisComponent chassisComp)
    {
        if (chassisComp.BrainEntity != null && HasComp<BorgAiBrainComponent>(chassisComp.BrainEntity.Value))
            EnableAi(chassis);
    }

    public void EnableAi(EntityUid chassis)
    {
        if (HasComp<BorgAiCommandComponent>(chassis))
            return;

        EnsureComp<BorgAiCommandComponent>(chassis);
        EnsureComp<NPCMeleeCombatComponent>(chassis);

        var htn = EnsureComp<HTNComponent>(chassis);
        htn.RootTask = new HTNCompoundTask { Task = RootTask };
        htn.Enabled = true;

        InitializeHtn(chassis, htn);
        _tools.PrepareForWork(chassis);
    }

    public void DisableAi(EntityUid chassis)
    {
        if (!HasComp<BorgAiCommandComponent>(chassis))
            return;

        RemComp<BorgAiCommandComponent>(chassis);

        if (TryComp<HTNComponent>(chassis, out var htn))
        {
            htn.Enabled = false;
            _htn.ShutdownPlan(htn);
            _npc.SleepNPC(chassis, htn);
            RemComp<HTNComponent>(chassis);
        }

        RemComp<NPCMeleeCombatComponent>(chassis);
    }

    private void InitializeHtn(EntityUid chassis, HTNComponent htn)
    {
        htn.Blackboard.SetValue(NPCBlackboard.Owner, chassis);
        _npc.SetBlackboard(chassis, NPCBlackboard.Owner, chassis);
        _npc.SetBlackboard(chassis, BorgAiBlackboard.Order, BorgAiOrderType.Idle);
        _npc.SetBlackboard(chassis, BorgAiBlackboard.CommandRange, BorgAiBlackboard.DefaultCommandRange);
        _npc.SetBlackboard(chassis, "InteractRange", SharedInteractionSystem.InteractionRange);
        _npc.SetBlackboard(chassis, "MovementRange", 1.5f);

        _npc.WakeNPC(chassis, htn);
        htn.PlanAccumulator = 0f;
        _htn.Replan(htn);
    }
}
