using Content.Server._Rat.Silicons.Borgs;
using Content.Server.Construction;
using Content.Server.Construction.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Construction.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared._Rat.Silicons.Borgs;

namespace Content.Server._Rat.NPC.HTN.Borg;

public sealed partial class BorgDisassembleOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private BorgAiToolSystem _tools = default!;
    private ConstructionSystem _construction = default!;
    private SharedInteractionSystem _interaction = default!;
    private SharedHandsSystem _hands = default!;

    [DataField(required: true)]
    public string TargetKey = BorgAiBlackboard.Target;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _tools = sysManager.GetEntitySystem<BorgAiToolSystem>();
        _construction = sysManager.GetEntitySystem<ConstructionSystem>();
        _interaction = sysManager.GetEntitySystem<SharedInteractionSystem>();
        _hands = sysManager.GetEntitySystem<SharedHandsSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || _entManager.Deleted(target)
            || !_entManager.TryGetComponent<ConstructionComponent>(target, out var construction))
        {
            return HTNOperatorStatus.Failed;
        }

        _tools.PrepareForWork(owner);

        if (!_tools.IsInInteractionRange(owner, target))
            return HTNOperatorStatus.Continuing;

        if (construction.DeconstructionNode == null)
            return HTNOperatorStatus.Failed;

        if (construction.Node == construction.DeconstructionNode)
            return HTNOperatorStatus.Finished;

        if (construction.TargetNode != construction.DeconstructionNode)
            _construction.SetPathfindingTarget(target, construction.DeconstructionNode, construction);

        _construction.UpdatePathfinding(target, construction);

        var coords = _entManager.GetComponent<TransformComponent>(target).Coordinates;

        if (!_entManager.TryGetComponent<HandsComponent>(owner, out var handsComp))
            return HTNOperatorStatus.Failed;

        foreach (var held in _hands.EnumerateHeld(owner, handsComp))
        {
            if (_interaction.InteractUsing(owner, held, target, coords, checkCanInteract: false, checkCanUse: false))
                return HTNOperatorStatus.Continuing;
        }

        return HTNOperatorStatus.Continuing;
    }
}
