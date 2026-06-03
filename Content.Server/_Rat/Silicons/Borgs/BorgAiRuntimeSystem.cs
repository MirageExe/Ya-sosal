using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Shared.NPC;
using Content.Server.NPC.Systems;
using Content.Shared.Interaction;
using Content.Shared._Rat.Silicons.Borgs;

namespace Content.Server._Rat.Silicons.Borgs;

/// <summary>
/// Keeps autonomous cyborg AI powered, awake, and replanning while it has an active order.
/// </summary>
public sealed class BorgAiRuntimeSystem : EntitySystem
{
    [Dependency] private readonly BorgAiToolSystem _tools = default!;
    [Dependency] private readonly HTNSystem _htn = default!;
    [Dependency] private readonly NPCSystem _npc = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BorgAiCommandComponent, HTNComponent>();
        while (query.MoveNext(out var uid, out var cmd, out var htn))
        {
            if (cmd.CurrentOrder == BorgAiOrderType.Idle)
                continue;

            _tools.PrepareForWork(uid);

            if (!HasComp<ActiveNPCComponent>(uid))
                _npc.WakeNPC(uid, htn);

            if (!htn.Enabled)
                htn.Enabled = true;

            if (htn.Plan == null && htn.PlanningJob == null)
            {
                htn.PlanAccumulator = 0f;
                _htn.Replan(htn);
            }
        }
    }
}
