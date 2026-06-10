using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._Rat.SpaceAnomaly;

[RegisterComponent]
public sealed partial class SpaceAnomalyFieldScannerComponent : Component
{
    [DataField("scanRange")]
    public float ScanRange = 10f;

    [DataField("scanDuration")]
    public float ScanDuration = 5f;
}

[Serializable, NetSerializable]
public sealed partial class SpaceAnomalyFieldScanDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class SpaceAnomalyStabilizerDoAfterEvent : SimpleDoAfterEvent
{
}
