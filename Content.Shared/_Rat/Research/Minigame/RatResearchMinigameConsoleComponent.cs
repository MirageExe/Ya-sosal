using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System.Collections.Generic;

namespace Content.Shared._Rat.Research.Minigame;

[RegisterComponent]
public sealed partial class RatResearchMinigameConsoleComponent : Component
{
    [DataField("currentGame")]
    public RatResearchMinigameType? CurrentGame;

    [DataField("gameState")]
    public Dictionary<string, object> GameState = new();

    [DataField("points")]
    public int Points = 0;

    [DataField("difficulty")]
    public int Difficulty = 1;

    [DataField("wireOrder")]
    public List<int> WireOrder = new();
}

[Serializable, NetSerializable]
public enum RatResearchMinigameType
{
    Tune,
    Sequence,
    PhaseLock,
    Harmonic,
    WaveformFilter,
    Waveform,
    CipherShift,
    Cipher,
    Timing,
    WirePort,
    Wire,
    Memory,
    BitCount,
    BitRepair
}
