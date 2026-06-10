using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._Rat.SpaceAnomaly;

[RegisterComponent]
public sealed partial class SpaceAnomalyComponent : Component
{
    [DataField("anomalyType")]
    public string AnomalyType = string.Empty;

    [DataField("stabilityLevel")]
    public float StabilityLevel = 1.0f;

    [DataField("isStabilized")]
    public bool IsStabilized = false;

    [DataField("studyProgress")]
    public float StudyProgress = 0f;

    [DataField("isBeingStudied")]
    public bool IsBeingStudied = false;

    [DataField("behaviorKind")]
    public SpaceAnomalyBehaviorKind BehaviorKind = SpaceAnomalyBehaviorKind.Neutral;

    [DataField("behavior")]
    public SpaceAnomalyBehaviorKind Behavior = SpaceAnomalyBehaviorKind.Neutral;

    [DataField("scale")]
    public float Scale = 1.0f;

    [DataField("fromEvent")]
    public bool FromEvent = false;

    [DataField("despawnTime")]
    public TimeSpan DespawnTime = TimeSpan.Zero;

    [DataField("nextBehaviorPulse")]
    public TimeSpan NextBehaviorPulse = TimeSpan.Zero;

    [DataField("studied")]
    public bool Studied = false;
}

[Serializable, NetSerializable]
public enum SpaceAnomalyBehaviorKind
{
    Neutral,
    Aggressive,
    Defensive,
    Evasive,
    Unstable,
    Dormant,
    Gravity,
    Bluespace,
    Pyro,
    Electric,
    Ice,
    Flesh,
    Shadow,
    Liquid,
    Flora
}
