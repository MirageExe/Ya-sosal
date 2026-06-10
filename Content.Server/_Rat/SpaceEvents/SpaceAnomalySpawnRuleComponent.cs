using Content.Server._Rat.SpaceEvents;

namespace Content.Server._Rat.SpaceEvents.Components;

[RegisterComponent, Access(typeof(SpaceAnomalySpawnRule))]
public sealed partial class SpaceAnomalySpawnRuleComponent : Component
{
    [DataField]
    public int SpawnCount = 2;

    [DataField]
    public LocId Announcement = "space-anomaly-event-announcement";
}
