using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Shared._Rat.Silicons.Borgs;

namespace Content.Server._Rat.NPC.HTN.Preconditions;

public sealed partial class HasBorgAiOrderPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField("order", required: true)]
    public BorgAiOrderType Order;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        return blackboard.TryGetValue<BorgAiOrderType>(BorgAiBlackboard.Order, out var order, _entManager)
            && order == Order;
    }
}
