using Content.Server._Rat.SpaceAnomaly;
using Content.Server._Rat.SpaceEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;

namespace Content.Server._Rat.SpaceEvents;

public sealed class SpaceAnomalySpawnRule : StationEventSystem<SpaceAnomalySpawnRuleComponent>
{
    [Dependency] private readonly SpaceAnomalySystem _spaceAnomaly = default!;

    protected override void Added(EntityUid uid, SpaceAnomalySpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, comp, gameRule, args);
        ChatSystem.DispatchGlobalAnnouncement(Loc.GetString(comp.Announcement), colorOverride: Color.MediumPurple);
    }

    protected override void Started(EntityUid uid, SpaceAnomalySpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        for (var i = 0; i < comp.SpawnCount; i++)
            _spaceAnomaly.TrySpawnRandom(fromEvent: true);
    }
}
