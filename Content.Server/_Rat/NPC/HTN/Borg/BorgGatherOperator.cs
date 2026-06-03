using Content.Server._Rat.Silicons.Borgs;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared._Rat.Silicons.Borgs;

namespace Content.Server._Rat.NPC.HTN.Borg;

public sealed partial class BorgGatherOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private BorgAiToolSystem _tools = default!;
    private SharedHandsSystem _hands = default!;
    private SharedInteractionSystem _interaction = default!;
    private EntityLookupSystem _lookup = default!;

    [DataField(required: true)]
    public string TargetKey = BorgAiBlackboard.Target;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _tools = sysManager.GetEntitySystem<BorgAiToolSystem>();
        _hands = sysManager.GetEntitySystem<SharedHandsSystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
        _lookup = sysManager.GetEntitySystem<EntityLookupSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        _tools.PrepareForWork(owner);

        EntityUid target;
        if (blackboard.TryGetValue<EntityUid>(TargetKey, out var ordered, _entManager) && !_entManager.Deleted(ordered))
        {
            target = ordered;
        }
        else
        {
            target = EntityUid.Invalid;
            foreach (var ent in _lookup.GetEntitiesInRange(owner, 4f))
            {
                if (!_entManager.HasComponent<ItemComponent>(ent))
                    continue;

                target = ent;
                break;
            }

            if (!target.IsValid())
                return HTNOperatorStatus.Failed;
        }

        if (!_tools.IsInInteractionRange(owner, target))
            return HTNOperatorStatus.Continuing;

        if (!_entManager.TryGetComponent<HandsComponent>(owner, out var handsComp))
            return HTNOperatorStatus.Failed;

        if (_hands.GetActiveHand(owner) is { } hand && hand.HeldEntity == null)
        {
            _hands.TryPickupAnyHand(owner, target, handsComp: handsComp);
            return HTNOperatorStatus.Finished;
        }

        _interaction.InteractHand(owner, target);
        return HTNOperatorStatus.Finished;
    }
}
