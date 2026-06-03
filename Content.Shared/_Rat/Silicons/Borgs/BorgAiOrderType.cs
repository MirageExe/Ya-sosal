using Robust.Shared.Serialization;

namespace Content.Shared._Rat.Silicons.Borgs;

/// <summary>
/// Contextual orders issued to a cyborg with an AI brain.
/// </summary>
[Serializable, NetSerializable]
public enum BorgAiOrderType : byte
{
    Idle = 0,
    Disassemble,
    Heal,
    Solve,
    Defend,
    Gather,
}
