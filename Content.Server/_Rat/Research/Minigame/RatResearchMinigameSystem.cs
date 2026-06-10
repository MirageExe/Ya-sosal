using Content.Server.Power.EntitySystems;
using Content.Server.Research.Systems;
using Content.Server._Rat.SpaceAnomaly;
using Content.Shared._Rat.Research.Minigame;
using Content.Shared.Popups;
using Content.Shared.Research.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._Rat.Research.Minigame;

public sealed class RatResearchMinigameSystem : EntitySystem
{
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, BoundUIOpenedEvent>(OnOpened);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchStartMinigameMessage>(OnStartMinigame);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitTuneBandMessage>(OnSubmitTuneBand);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitTuneMessage>(OnSubmitTune);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitTuneFinalizeMessage>(OnSubmitTuneFinalize);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitSequenceMessage>(OnSubmitSequence);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitPhaseLockMessage>(OnSubmitPhaseLock);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitHarmonicMessage>(OnSubmitHarmonic);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitWaveformFilterMessage>(OnSubmitWaveformFilter);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitWaveformMessage>(OnSubmitWaveform);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitCipherShiftMessage>(OnSubmitCipherShift);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitCipherMessage>(OnSubmitCipher);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitTimingMessage>(OnSubmitTiming);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitWirePortMessage>(OnSubmitWirePort);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitWireMessage>(OnSubmitWire);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitMemoryMessage>(OnSubmitMemory);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitBitCountMessage>(OnSubmitBitCount);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchSubmitBitRepairMessage>(OnSubmitBitRepair);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, RatResearchUnlockTechnologyMessage>(OnUnlockTechnology);
        SubscribeLocalEvent<RatResearchMinigameConsoleComponent, ResearchRegistrationChangedEvent>(OnRegistrationChanged);
    }

    private void OnRegistrationChanged(EntityUid uid, RatResearchMinigameConsoleComponent comp, ResearchRegistrationChangedEvent args) => UpdateUi(uid, comp);
    private void OnOpened(EntityUid uid, RatResearchMinigameConsoleComponent comp, BoundUIOpenedEvent args) => UpdateUi(uid, comp);

    private void OnStartMinigame(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchStartMinigameMessage args)
    {
        if (!_power.IsPowered(uid))
            return;

        comp.ActiveType = args.Type;
        comp.PuzzleActive = true;
        comp.PuzzleSeed = _random.Next();
        RatResearchMinigameStages.Setup(comp, _random);
        UpdateUi(uid, comp);
    }

    private void OnSubmitTuneBand(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitTuneBandMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.FrequencyTune, 0))
            return;

        if (args.Band != comp.TuneBandTarget)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.TuneReward);
    }

    private void OnSubmitTune(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitTuneMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.FrequencyTune, 1))
            return;

        if (Math.Abs((args.DialA + args.DialB + args.DialC) / 3f - comp.PuzzleTarget) > comp.TuneTolerance)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.TuneReward);
    }

    private void OnSubmitTuneFinalize(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitTuneFinalizeMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.FrequencyTune, 2))
            return;

        var avg = (args.DialA + args.DialB + args.DialC) / 3f;
        var mod = (args.DialA * args.DialB + args.DialC) % 100;

        if (Math.Abs(avg - comp.PuzzleTarget) > comp.TuneTolerance || mod != comp.PuzzleSecondaryTarget)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.TuneReward);
    }

    private void OnSubmitSequence(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitSequenceMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.SequenceMatch))
            return;

        var expected = RatResearchMinigameStages.TransformSequence(comp.PuzzleSequence, comp.SequenceTransformMode);
        if (!RatResearchMinigameStages.SequencesEqual(args.Sequence, expected))
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.SequenceReward);
    }

    private void OnSubmitPhaseLock(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitPhaseLockMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.PhaseLock))
            return;

        var sumOk = Math.Abs(args.PhaseA + args.PhaseB - comp.PuzzleTarget) <= comp.PhaseTolerance;

        if (comp.PuzzleStage == 0)
        {
            if (!sumOk)
            {
                Fail(uid);
                return;
            }

            AdvanceOrComplete(uid, comp, comp.PhaseLockReward);
            return;
        }

        if (!sumOk || Math.Abs(Math.Abs(args.PhaseA - args.PhaseB) - comp.PuzzleSecondaryTarget) > 2)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.PhaseLockReward);
    }

    private void OnSubmitHarmonic(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitHarmonicMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.HarmonicBalance))
            return;

        if (args.Stability + args.Severity != 100)
        {
            Fail(uid);
            return;
        }

        if (comp.PuzzleStage == 0)
        {
            if (Math.Abs(args.Stability - comp.PuzzleTarget) > 5)
            {
                Fail(uid);
                return;
            }

            AdvanceOrComplete(uid, comp, comp.HarmonicReward);
            return;
        }

        if (Math.Abs(args.Stability * args.Severity - comp.PuzzleSecondaryTarget) > comp.HarmonicProductTolerance)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.HarmonicReward);
    }

    private void OnSubmitWaveformFilter(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitWaveformFilterMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.WaveformPick, 0))
            return;

        if (args.NoiseIndices.Count != 2 || !MatchesNoiseFilter(args.NoiseIndices, comp.WaveformNoiseIndices))
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.WaveformReward);
    }

    private void OnSubmitWaveform(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitWaveformMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.WaveformPick, 1))
            return;

        if (args.SelectedIndex != comp.WaveformCorrectIndex)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.WaveformReward);
    }

    private void OnSubmitCipherShift(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitCipherShiftMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.CipherDecrypt, 0))
            return;

        if (args.Shift != comp.CipherShift)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.CipherReward);
    }

    private void OnSubmitCipher(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitCipherMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.CipherDecrypt, 1))
            return;

        if (args.SelectedIndex != comp.CipherAnswerIndex)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.CipherReward);
    }

    private void OnSubmitTiming(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitTimingMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.TimingPulse))
            return;

        var center = (comp.TimingZoneLow + comp.TimingZoneHigh) / 2;

        if (comp.PuzzleStage == 0)
        {
            if (Math.Abs(args.Position - center) > 12)
            {
                Fail(uid);
                return;
            }

            AdvanceOrComplete(uid, comp, comp.TimingReward);
            return;
        }

        if (args.Position < comp.TimingZoneLow || args.Position > comp.TimingZoneHigh)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.TimingReward);
    }

    private void OnSubmitWirePort(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitWirePortMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.WireRouting, 0))
            return;

        if (comp.WireOrder.Count == 0 || args.Port != comp.WireOrder[0])
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.WireReward);
    }

    private void OnSubmitWire(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitWireMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.WireRouting, 1))
            return;

        if (args.Mapping.Count != 4)
        {
            Fail(uid);
            return;
        }

        for (var i = 0; i < 4; i++)
        {
            if (args.Mapping[i] != comp.WireOrder[i])
            {
                Fail(uid);
                return;
            }
        }

        AdvanceOrComplete(uid, comp, comp.WireReward);
    }

    private void OnSubmitMemory(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitMemoryMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.MemoryGrid))
            return;

        var expected = comp.SequenceTransformMode == 1
            ? RatResearchMinigameStages.TransformSequence(comp.MemoryPattern, 1)
            : comp.MemoryPattern;

        if (!RatResearchMinigameStages.SequencesEqual(args.Sequence, expected))
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.MemoryReward);
    }

    private void OnSubmitBitCount(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitBitCountMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.BitRepair, 0))
            return;

        if (args.Count != comp.PuzzleTarget)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.BitRepairReward);
    }

    private void OnSubmitBitRepair(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchSubmitBitRepairMessage args)
    {
        if (!CanSubmit(uid, comp, RatResearchMinigameType.BitRepair, 1))
            return;

        if (args.Mask != comp.BitTarget)
        {
            Fail(uid);
            return;
        }

        AdvanceOrComplete(uid, comp, comp.BitRepairReward);
    }

    private void OnUnlockTechnology(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchUnlockTechnologyMessage args)
    {
        if (!_power.IsPowered(uid))
            return;

        _research.UnlockTechnology(uid, args.TechnologyId, args.Actor);
        UpdateUi(uid, comp);
    }

    private bool CanSubmit(EntityUid uid, RatResearchMinigameConsoleComponent comp, RatResearchMinigameType type, int? stage = null)
    {
        if (!comp.PuzzleActive || comp.ActiveType != type || !_power.IsPowered(uid))
            return false;

        return stage == null || comp.PuzzleStage == stage.Value;
    }

    private void AdvanceOrComplete(EntityUid uid, RatResearchMinigameConsoleComponent comp, int reward)
    {
        if (comp.PuzzleStage + 1 >= comp.PuzzleStageCount)
        {
            comp.PuzzleActive = false;
            Success(uid, reward);
            return;
        }

        RatResearchMinigameStages.AdvanceStage(comp);
        _popup.PopupEntity(Loc.GetString("rat-research-minigame-stage-advance",
            ("current", comp.PuzzleStage + 1),
            ("total", comp.PuzzleStageCount)), uid, PopupType.Small);
        UpdateUi(uid, comp);
    }

    private static bool MatchesNoiseFilter(List<int> submitted, List<int> expected)
    {
        if (submitted.Count != expected.Count)
            return false;

        var a = new List<int>(submitted);
        var b = new List<int>(expected);
        a.Sort();
        b.Sort();

        for (var i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    private void Success(EntityUid uid, int reward)
    {
        GrantPoints(uid, reward);
        _popup.PopupEntity(Loc.GetString("rat-research-minigame-success", ("points", reward)), uid, PopupType.Medium);
        if (TryComp<RatResearchMinigameConsoleComponent>(uid, out var comp))
            UpdateUi(uid, comp);
    }

    private void Fail(EntityUid uid)
    {
        if (TryComp<RatResearchMinigameConsoleComponent>(uid, out var comp))
            comp.PuzzleActive = false;

        _popup.PopupEntity(Loc.GetString("rat-research-minigame-fail"), uid, PopupType.Medium);
        if (TryComp<RatResearchMinigameConsoleComponent>(uid, out comp))
            UpdateUi(uid, comp);
    }

    private void GrantPoints(EntityUid console, int points)
    {
        if (!TryComp<ResearchClientComponent>(console, out var client) || client.Server is not { } server)
            return;

        if (!TryComp<ResearchServerComponent>(server, out var serverComp))
            return;

        _research.ModifyServerPoints(server, points, serverComp);
    }

    private void UpdateUi(EntityUid uid, RatResearchMinigameConsoleComponent comp)
    {
        var points = 0;
        var hintLow = 0;
        var hintHigh = 0;
        var hintSecLow = 0;
        var hintSecHigh = 0;
        var sequence = new List<int>();
        var waveforms = new List<string>();
        var cipherOptions = new List<string>();
        var cipherText = string.Empty;
        var memory = new List<int>();
        var wireOrder = new List<int>();

        if (TryComp<ResearchClientComponent>(uid, out var client) && client.Server is { } server &&
            TryComp<ResearchServerComponent>(server, out var serverComp))
        {
            points = serverComp.Points;
        }

        if (comp.PuzzleActive)
        {
            switch (comp.ActiveType)
            {
                case RatResearchMinigameType.FrequencyTune:
                    if (comp.PuzzleStage == 0)
                    {
                        hintLow = RatResearchMinigameStages.GetTuneBandHintLow(comp.TuneBandTarget);
                        hintHigh = RatResearchMinigameStages.GetTuneBandHintHigh(comp.TuneBandTarget);
                    }
                    else
                    {
                        hintLow = Math.Max(0, comp.PuzzleTarget - comp.TuneTolerance * 2);
                        hintHigh = Math.Min(100, comp.PuzzleTarget + comp.TuneTolerance * 2);
                        hintSecLow = comp.PuzzleSecondaryTarget - 3;
                        hintSecHigh = comp.PuzzleSecondaryTarget + 3;
                    }

                    break;
                case RatResearchMinigameType.SequenceMatch:
                    sequence = new List<int>(comp.PuzzleSequence);
                    break;
                case RatResearchMinigameType.PhaseLock:
                    hintLow = comp.PuzzleTarget - comp.PhaseTolerance;
                    hintHigh = comp.PuzzleTarget + comp.PhaseTolerance;
                    hintSecLow = comp.PuzzleSecondaryTarget - 2;
                    hintSecHigh = comp.PuzzleSecondaryTarget + 2;
                    break;
                case RatResearchMinigameType.HarmonicBalance:
                    hintLow = comp.PuzzleTarget - 5;
                    hintHigh = comp.PuzzleTarget + 5;
                    hintSecLow = comp.PuzzleSecondaryTarget - comp.HarmonicProductTolerance;
                    hintSecHigh = comp.PuzzleSecondaryTarget + comp.HarmonicProductTolerance;
                    break;
                case RatResearchMinigameType.WaveformPick:
                    waveforms = SpaceAnomalyStudyPuzzles.BuildWaveformOptions();
                    break;
                case RatResearchMinigameType.CipherDecrypt:
                    if (comp.PuzzleStage >= 1)
                    {
                        cipherOptions = new List<string>(comp.CipherOptions);
                        if (comp.CipherAnswerIndex >= 0 && comp.CipherAnswerIndex < comp.CipherOptions.Count)
                            cipherText = SpaceAnomalyStudyPuzzles.EncodeCipher(comp.CipherOptions[comp.CipherAnswerIndex], comp.CipherShift);
                    }

                    break;
                case RatResearchMinigameType.TimingPulse:
                    hintLow = comp.TimingZoneLow;
                    hintHigh = comp.TimingZoneHigh;
                    break;
                case RatResearchMinigameType.WireRouting:
                    wireOrder = new List<int> { 0, 1, 2, 3 };
                    break;
                case RatResearchMinigameType.MemoryGrid:
                    memory = new List<int>(comp.MemoryPattern);
                    break;
                case RatResearchMinigameType.BitRepair:
                    hintLow = comp.PuzzleTarget;
                    break;
            }
        }

        var state = new RatResearchMinigameBuiState
        {
            ServerPoints = points,
            ActiveType = comp.ActiveType,
            PuzzleSeed = comp.PuzzleSeed,
            HintLow = hintLow,
            HintHigh = hintHigh,
            HintSecondaryLow = hintSecLow,
            HintSecondaryHigh = hintSecHigh,
            Sequence = sequence,
            WaveformOptions = waveforms,
            CipherOptions = cipherOptions,
            CipherText = cipherText,
            CipherShift = comp.CipherShift,
            MemoryPattern = memory,
            WireOrder = wireOrder,
            BitMask = comp.BitMask,
            BitTarget = comp.BitTarget,
            TimingZoneLow = comp.TimingZoneLow,
            TimingZoneHigh = comp.TimingZoneHigh,
            PuzzleActive = comp.PuzzleActive,
            PuzzleStage = comp.PuzzleStage,
            PuzzleStageCount = comp.PuzzleStageCount,
            TuneBandTarget = comp.TuneBandTarget,
            SequenceTransformMode = comp.SequenceTransformMode,
            WaveformNoiseIndices = new List<int>(comp.WaveformNoiseIndices),
            CipherShiftOptions = new List<int>(comp.CipherShiftOptions),
        };

        _ui.SetUiState(uid, RatResearchMinigameUiKey.Key, state);
    }
}
