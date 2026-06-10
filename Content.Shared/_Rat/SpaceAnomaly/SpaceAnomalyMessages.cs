using Robust.Shared.Serialization;

namespace Content.Shared._Rat.SpaceAnomaly;

[Serializable, NetSerializable]
public sealed class SpaceAnomalyRefreshMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySelectTargetMessage : BoundUserInterfaceMessage
{
    public EntityUid Target { get; }

    public SpaceAnomalySelectTargetMessage(EntityUid target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalyBeginExpeditionMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySubmitPhaseLockMessage : BoundUserInterfaceMessage
{
    public int Value { get; }
    public int PhaseA { get; }
    public int PhaseB { get; }

    public SpaceAnomalySubmitPhaseLockMessage(int value, int phaseA = 0, int phaseB = 0)
    {
        Value = value;
        PhaseA = phaseA;
        PhaseB = phaseB;
    }
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySubmitCipherMessage : BoundUserInterfaceMessage
{
    public string Code { get; }
    public int SelectedIndex { get; }

    public SpaceAnomalySubmitCipherMessage(string code, int selectedIndex = 0)
    {
        Code = code;
        SelectedIndex = selectedIndex;
    }
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySubmitTimingMessage : BoundUserInterfaceMessage
{
    public List<float> Timings { get; }
    public float Position { get; }

    public SpaceAnomalySubmitTimingMessage(List<float> timings, float position = 0f)
    {
        Timings = timings;
        Position = position;
    }
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySubmitWireMessage : BoundUserInterfaceMessage
{
    public int WireId { get; }
    public Dictionary<int, int> Mapping { get; }

    public SpaceAnomalySubmitWireMessage(int wireId, Dictionary<int, int>? mapping = null)
    {
        WireId = wireId;
        Mapping = mapping ?? new Dictionary<int, int>();
    }
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySubmitMemoryMessage : BoundUserInterfaceMessage
{
    public List<int> Sequence { get; }

    public SpaceAnomalySubmitMemoryMessage(List<int> sequence)
    {
        Sequence = sequence;
    }
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySubmitHarmonicMessage : BoundUserInterfaceMessage
{
    public float Frequency { get; }
    public float Stability { get; }
    public float Severity { get; }

    public SpaceAnomalySubmitHarmonicMessage(float frequency, float stability = 0f, float severity = 0f)
    {
        Frequency = frequency;
        Stability = stability;
        Severity = severity;
    }
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalySubmitWaveformMessage : BoundUserInterfaceMessage
{
    public int Pattern { get; }

    public SpaceAnomalySubmitWaveformMessage(int pattern)
    {
        Pattern = pattern;
    }
}

[Serializable, NetSerializable]
public enum SpaceAnomalyStudyConsoleUiKey
{
    Key
}
