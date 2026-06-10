using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System.Collections.Generic;

namespace Content.Shared._Rat.SpaceAnomaly;

[RegisterComponent]
public sealed partial class SpaceAnomalyStudyConsoleComponent : Component
{
    [DataField("selectedTarget")]
    public EntityUid? SelectedTarget;

    [DataField("activeTarget")]
    public EntityUid? ActiveTarget;

    [DataField("currentPuzzle")]
    public SpaceAnomalyStudyStepKind? CurrentPuzzle;

    [DataField("puzzleState")]
    public Dictionary<string, object> PuzzleState = new();

    [DataField("studyProgress")]
    public float StudyProgress = 0f;

    [DataField("isStudying")]
    public bool IsStudying = false;

    [DataField("stepIndex")]
    public int StepIndex = 0;

    [DataField("expeditionSteps")]
    public List<SpaceAnomalyStudyStepKind> ExpeditionSteps = new();

    [DataField("puzzleActive")]
    public bool PuzzleActive = false;

    [DataField("phaseTargetSum")]
    public int PhaseTargetSum = 0;

    [DataField("phaseTolerance")]
    public int PhaseTolerance = 0;

    [DataField("cipherAnswerIndex")]
    public int CipherAnswerIndex = 0;

    [DataField("timingZoneLow")]
    public float TimingZoneLow = 0f;

    [DataField("timingZoneHigh")]
    public float TimingZoneHigh = 0f;

    [DataField("memoryPattern")]
    public List<int> MemoryPattern = new();

    [DataField("harmonicProductTarget")]
    public float HarmonicProductTarget = 0f;

    [DataField("harmonicProductTolerance")]
    public float HarmonicProductTolerance = 0f;
}

[Serializable, NetSerializable]
public enum SpaceAnomalyStudyStepKind
{
    PhaseLock,
    RemotePhaseLock,
    Cipher,
    RemoteCipher,
    Timing,
    RemoteTimingPulse,
    Wire,
    RemoteWireRoute,
    Memory,
    RemoteMemoryGrid,
    Harmonic,
    RemoteHarmonic,
    Waveform
}

[Serializable, NetSerializable]
public sealed class SpaceAnomalyStudyEntry
{
    public EntityUid AnomalyUid { get; set; }
    public string AnomalyType { get; set; } = string.Empty;
    public float Progress { get; set; }
    public float Stability { get; set; }
}
