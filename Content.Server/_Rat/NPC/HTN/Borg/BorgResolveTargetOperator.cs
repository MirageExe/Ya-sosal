using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared._Rat.Silicons.Borgs;
using Robust.Shared.Map;

namespace Content.Server._Rat.NPC.HTN.Borg;

/// <summary>
/// Writes <see cref="NPCBlackboard.PathfindKey"/> coordinates from an entity target.
/// </summary>
public sealed partial class BorgResolveTargetOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField(required: true)]
    public string TargetKey = BorgAiBlackboard.Target;

    [DataField]
    public string CoordinatesKey = "TargetCoordinates";

    public override Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(
        NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || _entManager.Deleted(target))
        {
            return Task.FromResult<(bool, Dictionary<string, object>?)>((false, null));
        }

        var coords = _entManager.GetComponent<TransformComponent>(target).Coordinates;
        return Task.FromResult<(bool, Dictionary<string, object>?)>((true, new Dictionary<string, object>
        {
            { CoordinatesKey, coords },
        }));
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || _entManager.Deleted(target))
        {
            return HTNOperatorStatus.Failed;
        }

        blackboard.SetValue(CoordinatesKey, _entManager.GetComponent<TransformComponent>(target).Coordinates);
        return HTNOperatorStatus.Finished;
    }
}
