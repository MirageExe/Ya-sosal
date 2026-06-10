using Robust.Shared.Serialization;
using System.Collections.Generic;

namespace Content.Shared._Rat.Research.Minigame;

[Serializable, NetSerializable]
public sealed class RatResearchStartMinigameMessage : BoundUserInterfaceMessage
{
    public RatResearchMinigameType GameType { get; }

    public RatResearchStartMinigameMessage(RatResearchMinigameType gameType)
    {
        GameType = gameType;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitTuneBandMessage : BoundUserInterfaceMessage
{
    public int BandIndex { get; }

    public RatResearchSubmitTuneBandMessage(int bandIndex)
    {
        BandIndex = bandIndex;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitTuneMessage : BoundUserInterfaceMessage
{
    public float Frequency { get; }

    public RatResearchSubmitTuneMessage(float frequency)
    {
        Frequency = frequency;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitTuneFinalizeMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitSequenceMessage : BoundUserInterfaceMessage
{
    public List<int> Sequence { get; }

    public RatResearchSubmitSequenceMessage(List<int> sequence)
    {
        Sequence = sequence;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitPhaseLockMessage : BoundUserInterfaceMessage
{
    public int Value { get; }

    public RatResearchSubmitPhaseLockMessage(int value)
    {
        Value = value;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitHarmonicMessage : BoundUserInterfaceMessage
{
    public float Frequency { get; }

    public RatResearchSubmitHarmonicMessage(float frequency)
    {
        Frequency = frequency;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitWaveformFilterMessage : BoundUserInterfaceMessage
{
    public int FilterType { get; }

    public RatResearchSubmitWaveformFilterMessage(int filterType)
    {
        FilterType = filterType;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitWaveformMessage : BoundUserInterfaceMessage
{
    public int Pattern { get; }

    public RatResearchSubmitWaveformMessage(int pattern)
    {
        Pattern = pattern;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitCipherShiftMessage : BoundUserInterfaceMessage
{
    public int Shift { get; }

    public RatResearchSubmitCipherShiftMessage(int shift)
    {
        Shift = shift;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitCipherMessage : BoundUserInterfaceMessage
{
    public string Code { get; }

    public RatResearchSubmitCipherMessage(string code)
    {
        Code = code;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitTimingMessage : BoundUserInterfaceMessage
{
    public List<float> Timings { get; }

    public RatResearchSubmitTimingMessage(List<float> timings)
    {
        Timings = timings;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitWirePortMessage : BoundUserInterfaceMessage
{
    public int Port { get; }

    public RatResearchSubmitWirePortMessage(int port)
    {
        Port = port;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitWireMessage : BoundUserInterfaceMessage
{
    public int WireId { get; }

    public RatResearchSubmitWireMessage(int wireId)
    {
        WireId = wireId;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitMemoryMessage : BoundUserInterfaceMessage
{
    public List<int> Sequence { get; }

    public RatResearchSubmitMemoryMessage(List<int> sequence)
    {
        Sequence = sequence;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitBitCountMessage : BoundUserInterfaceMessage
{
    public int Count { get; }

    public RatResearchSubmitBitCountMessage(int count)
    {
        Count = count;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchSubmitBitRepairMessage : BoundUserInterfaceMessage
{
    public int Position { get; }

    public RatResearchSubmitBitRepairMessage(int position)
    {
        Position = position;
    }
}

[Serializable, NetSerializable]
public sealed class RatResearchUnlockTechnologyMessage : BoundUserInterfaceMessage
{
    public string TechId { get; }

    public RatResearchUnlockTechnologyMessage(string techId)
    {
        TechId = techId;
    }
}

[Serializable, NetSerializable]
public enum RatResearchMinigameConsoleUiKey
{
    Key
}
