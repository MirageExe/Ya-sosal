using Content.Server.Power.EntitySystems;
using Content.Server.Research.Systems;
using Content.Shared._Rat.SpaceAnomaly;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Research.Components;
using Content.Shared.Tools.Components;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._Rat.SpaceAnomaly;

public sealed class SpaceAnomalyStudySystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, BoundUIOpenedEvent>(OnConsoleOpened);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalyRefreshMessage>(OnRefresh);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySelectTargetMessage>(OnSelectTarget);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalyBeginExpeditionMessage>(OnBeginExpedition);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySubmitPhaseLockMessage>(OnSubmitPhaseLock);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySubmitCipherMessage>(OnSubmitCipher);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySubmitTimingMessage>(OnSubmitTiming);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySubmitWireMessage>(OnSubmitWire);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySubmitMemoryMessage>(OnSubmitMemory);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySubmitHarmonicMessage>(OnSubmitHarmonic);
        SubscribeLocalEvent<SpaceAnomalyStudyConsoleComponent, SpaceAnomalySubmitWaveformMessage>(OnSubmitWaveform);

        SubscribeLocalEvent<SpaceAnomalyFieldScannerComponent, AfterInteractEvent>(OnScannerAfterInteract);
        SubscribeLocalEvent<SpaceAnomalyFieldScannerComponent, SpaceAnomalyFieldScanDoAfterEvent>(OnFieldScanDoAfter);

        SubscribeLocalEvent<SpaceAnomalyComponent, InteractUsingEvent>(OnAnomalyInteractUsing);
        SubscribeLocalEvent<SpaceAnomalyComponent, SpaceAnomalyStabilizerDoAfterEvent>(OnStabilizerDoAfter);
    }

    private void OnConsoleOpened(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, BoundUIOpenedEvent args) => UpdateUi(uid, comp);
    private void OnRefresh(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalyRefreshMessage args) => UpdateUi(uid, comp);

    private void OnSelectTarget(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySelectTargetMessage args)
    {
        if (comp.StepIndex >= 0)
            return;

        if (!TryGetEntity(args.Target, out var target) || target is not { } targetUid || !HasComp<SpaceAnomalyComponent>(targetUid))
            return;

        if (!IsOnShuttleGrid(uid) || !IsTargetInRange(uid, comp, targetUid))
            return;

        ResetExpedition(comp);
        comp.ActiveTarget = targetUid;
        UpdateUi(uid, comp);
    }

    private void OnBeginExpedition(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalyBeginExpeditionMessage args)
    {
        if (comp.StepIndex >= 0 || comp.ActiveTarget is not { } target || !Exists(target))
            return;

        if (!_power.IsPowered(uid) || !IsOnShuttleGrid(uid) || !IsTargetInRange(uid, comp, target))
            return;

        if (!TryComp<SpaceAnomalyComponent>(target, out var space) || space.Studied)
            return;

        comp.ExpeditionSteps = SpaceAnomalyStudyPuzzles.BuildExpedition(_random, space.Behavior);
        comp.StepIndex = 0;
        SpaceAnomalyStudyPuzzles.SetupStep(comp, comp.ExpeditionSteps[0], _random);

        _popup.PopupEntity(Loc.GetString("space-anomaly-study-expedition-start",
            ("steps", comp.ExpeditionSteps.Count)), uid, PopupType.Medium);
        UpdateUi(uid, comp);
    }

    private void OnSubmitPhaseLock(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySubmitPhaseLockMessage args)
    {
        if (GetCurrentStep(comp) != SpaceAnomalyStudyStepKind.RemotePhaseLock || !comp.PuzzleActive || !ValidateActiveTarget(uid, comp, out _))
            return;

        if (Math.Abs(args.PhaseA + args.PhaseB - comp.PhaseTargetSum) > comp.PhaseTolerance)
        {
            FailStage(uid, comp);
            return;
        }

        AdvanceStep(uid, comp);
    }

    private void OnSubmitCipher(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySubmitCipherMessage args)
    {
        if (GetCurrentStep(comp) != SpaceAnomalyStudyStepKind.RemoteCipher || !comp.PuzzleActive || !ValidateActiveTarget(uid, comp, out _))
            return;

        if (args.SelectedIndex != comp.CipherAnswerIndex)
        {
            FailStage(uid, comp);
            return;
        }

        AdvanceStep(uid, comp);
    }

    private void OnSubmitTiming(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySubmitTimingMessage args)
    {
        if (GetCurrentStep(comp) != SpaceAnomalyStudyStepKind.RemoteTimingPulse || !comp.PuzzleActive || !ValidateActiveTarget(uid, comp, out _))
            return;

        if (args.Position < comp.TimingZoneLow || args.Position > comp.TimingZoneHigh)
        {
            FailStage(uid, comp);
            return;
        }

        AdvanceStep(uid, comp);
    }

    private void OnSubmitWire(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySubmitWireMessage args)
    {
        if (GetCurrentStep(comp) != SpaceAnomalyStudyStepKind.RemoteWireRoute || !comp.PuzzleActive || !ValidateActiveTarget(uid, comp, out _))
            return;

        if (args.Mapping.Count != 4 || !ValidateWire(comp, args.Mapping))
        {
            FailStage(uid, comp);
            return;
        }

        AdvanceStep(uid, comp);
    }

    private void OnSubmitMemory(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySubmitMemoryMessage args)
    {
        if (GetCurrentStep(comp) != SpaceAnomalyStudyStepKind.RemoteMemoryGrid || !comp.PuzzleActive || !ValidateActiveTarget(uid, comp, out _))
            return;

        if (args.Sequence.Count != comp.MemoryPattern.Count)
        {
            FailStage(uid, comp);
            return;
        }

        for (var i = 0; i < comp.MemoryPattern.Count; i++)
        {
            if (args.Sequence[i] != comp.MemoryPattern[i])
            {
                FailStage(uid, comp);
                return;
            }
        }

        AdvanceStep(uid, comp);
    }

    private void OnSubmitHarmonic(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySubmitHarmonicMessage args)
    {
        if (GetCurrentStep(comp) != SpaceAnomalyStudyStepKind.RemoteHarmonic || !comp.PuzzleActive || !ValidateActiveTarget(uid, comp, out _))
            return;

        if (args.Stability + args.Severity != 100)
        {
            FailStage(uid, comp);
            return;
        }

        var product = args.Stability * args.Severity;
        if (Math.Abs(product - comp.HarmonicProductTarget) > comp.HarmonicProductTolerance)
        {
            FailStage(uid, comp);
            return;
        }

        AdvanceStep(uid, comp);
    }

    private void OnSubmitWaveform(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalySubmitWaveformMessage args)
    {
        if (GetCurrentStep(comp) != SpaceAnomalyStudyStepKind.RemoteWaveformArchive || !comp.PuzzleActive || !ValidateActiveTarget(uid, comp, out var target))
            return;

        if (args.SelectedIndex != comp.WaveformCorrectIndex)
        {
            FailStage(uid, comp);
            return;
        }

        CompleteStudy(uid, comp, target);
    }

    private void OnScannerAfterInteract(EntityUid uid, SpaceAnomalyFieldScannerComponent comp, AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { } anomaly || !args.CanReach)
            return;

        var user = args.User;
        if (!HasComp<SpaceAnomalyComponent>(anomaly))
            return;

        var console = FindLinkedConsole(anomaly);
        if (console == null)
        {
            _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-no-mission"), user, user);
            return;
        }

        var (consoleUid, consoleComp) = console.Value;
        if (consoleComp.ActiveTarget != anomaly)
        {
            _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-wrong-target"), user, user);
            return;
        }

        if (!IsOutsideShuttle(user, consoleUid))
        {
            _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-must-leave-shuttle"), user, user);
            return;
        }

        var step = GetCurrentStep(consoleComp);
        var dist = GetDistance(user, anomaly);

        if (step == SpaceAnomalyStudyStepKind.EvaFieldScan)
        {
            if (dist < comp.FieldScanMinDistance || dist > comp.FieldScanMaxDistance)
            {
                _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-field-range",
                    ("min", (int) comp.FieldScanMinDistance),
                    ("max", (int) comp.FieldScanMaxDistance)), user, user);
                return;
            }

            StartScan(uid, user, anomaly, comp.FieldScanDuration, false, false);
            args.Handled = true;
            return;
        }

        if (step == SpaceAnomalyStudyStepKind.EvaCloseProbe)
        {
            if (dist > comp.CloseProbeMaxDistance)
            {
                _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-close-range",
                    ("max", (int) comp.CloseProbeMaxDistance)), user, user);
                return;
            }

            StartScan(uid, user, anomaly, comp.CloseProbeDuration, true, false);
            args.Handled = true;
            return;
        }

        if (step == SpaceAnomalyStudyStepKind.EvaSpectralScan)
        {
            if (dist > comp.CloseProbeMaxDistance)
            {
                _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-close-range",
                    ("max", (int) comp.CloseProbeMaxDistance)), user, user);
                return;
            }

            StartScan(uid, user, anomaly, comp.SpectralScanDuration, false, true);
            args.Handled = true;
        }
    }

    private void OnAnomalyInteractUsing(EntityUid uid, SpaceAnomalyComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        var console = FindLinkedConsole(uid);
        if (console == null)
            return;

        var (consoleUid, consoleComp) = console.Value;
        if (consoleComp.ActiveTarget != uid || !IsOutsideShuttle(args.User, consoleUid))
            return;

        var step = GetCurrentStep(consoleComp);
        if (step == SpaceAnomalyStudyStepKind.EvaChemicalInject)
        {
            if (TryInjectChemical(args.Used, consoleComp))
            {
                _popup.PopupEntity(Loc.GetString("space-anomaly-study-chemical-success",
                    ("reagent", consoleComp.RequiredReagent)), args.User, args.User);
                AdvanceStep(consoleUid, consoleComp);
                args.Handled = true;
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("space-anomaly-study-chemical-fail",
                    ("reagent", consoleComp.RequiredReagent),
                    ("amount", (int) consoleComp.RequiredReagentUnits)), args.User, args.User);
            }

            return;
        }

        if (step == SpaceAnomalyStudyStepKind.EvaStabilizerDeploy && HasComp<ToolComponent>(args.Used))
        {
            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(10),
                new SpaceAnomalyStabilizerDoAfterEvent(), uid, uid, args.Used)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                DistanceThreshold = 12f,
            });

            _popup.PopupEntity(Loc.GetString("space-anomaly-study-stabilizer-started"), args.User, args.User);
            args.Handled = true;
        }
    }

    private void OnStabilizerDoAfter(EntityUid uid, SpaceAnomalyComponent comp, SpaceAnomalyStabilizerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } anomaly)
            return;

        var console = FindLinkedConsole(anomaly);
        if (console == null || GetCurrentStep(console.Value.Item2) != SpaceAnomalyStudyStepKind.EvaStabilizerDeploy)
            return;

        _popup.PopupEntity(Loc.GetString("space-anomaly-study-stabilizer-complete"), args.User, args.User);
        AdvanceStep(console.Value.Item1, console.Value.Item2);
        args.Handled = true;
    }

    private void OnFieldScanDoAfter(EntityUid uid, SpaceAnomalyFieldScannerComponent comp, SpaceAnomalyFieldScanDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } anomaly)
            return;

        var user = args.User;
        var console = FindLinkedConsole(anomaly);
        if (console == null)
            return;

        var (consoleUid, consoleComp) = console.Value;
        if (consoleComp.ActiveTarget != anomaly)
            return;

        var step = GetCurrentStep(consoleComp);
        if (args.Spectral)
        {
            if (step != SpaceAnomalyStudyStepKind.EvaSpectralScan)
                return;

            _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-spectral-complete"), user, user);
            AdvanceStep(consoleUid, consoleComp);
        }
        else if (args.CloseProbe)
        {
            if (step != SpaceAnomalyStudyStepKind.EvaCloseProbe)
                return;

            _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-probe-complete"), user, user);
            AdvanceStep(consoleUid, consoleComp);
        }
        else
        {
            if (step != SpaceAnomalyStudyStepKind.EvaFieldScan)
                return;

            _popup.PopupEntity(Loc.GetString("space-anomaly-scanner-field-complete"), user, user);
            AdvanceStep(consoleUid, consoleComp);
        }

        _audio.PlayPvs("/Audio/Items/locator_beep.ogg", uid, AudioParams.Default.WithVolume(-4f));
        args.Handled = true;
    }

    private bool TryInjectChemical(EntityUid used, SpaceAnomalyStudyConsoleComponent comp)
    {
        var amount = _solution.GetTotalPrototypeQuantity(used, comp.RequiredReagent);
        return amount >= comp.RequiredReagentUnits;
    }

    private void AdvanceStep(EntityUid console, SpaceAnomalyStudyConsoleComponent comp)
    {
        comp.StepIndex++;
        if (comp.StepIndex >= comp.ExpeditionSteps.Count)
        {
            if (comp.ActiveTarget is { } target)
                CompleteStudy(console, comp, target);
            return;
        }

        var step = comp.ExpeditionSteps[comp.StepIndex];
        SpaceAnomalyStudyPuzzles.SetupStep(comp, step, _random);
        _popup.PopupEntity(Loc.GetString(GetStepLocale(step)), console, PopupType.Medium);
        UpdateUi(console, comp);
    }

    private void StartScan(EntityUid scanner, EntityUid user, EntityUid anomaly, TimeSpan duration, bool closeProbe, bool spectral)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, duration,
            new SpaceAnomalyFieldScanDoAfterEvent(closeProbe, spectral), scanner, anomaly, scanner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            RequireCanInteract = true,
            DistanceThreshold = spectral || closeProbe ? 12f : 25f,
        });

        _popup.PopupEntity(Loc.GetString(spectral
            ? "space-anomaly-scanner-spectral-started"
            : closeProbe
                ? "space-anomaly-scanner-probe-started"
                : "space-anomaly-scanner-field-started"), user, user);
    }

    private void CompleteStudy(EntityUid console, SpaceAnomalyStudyConsoleComponent comp, EntityUid target)
    {
        if (!TryComp<SpaceAnomalyComponent>(target, out var space) || space.Studied)
            return;

        space.Studied = true;
        Dirty(target, space);

        if (TryComp<ResearchClientComponent>(console, out var client)
            && client.Server is { } server
            && TryComp<ResearchServerComponent>(server, out var serverComp))
        {
            _research.ModifyServerPoints(server, space.StudyPointReward, serverComp);
        }

        _popup.PopupEntity(Loc.GetString("space-anomaly-study-complete",
            ("points", space.StudyPointReward)), console, PopupType.Large);

        ResetExpedition(comp);
        UpdateUi(console, comp);
    }

    private void FailStage(EntityUid console, SpaceAnomalyStudyConsoleComponent comp)
    {
        _popup.PopupEntity(Loc.GetString("space-anomaly-study-fail"), console, PopupType.Medium);
        ResetExpedition(comp);
        UpdateUi(console, comp);
    }

    private void ResetExpedition(SpaceAnomalyStudyConsoleComponent comp)
    {
        comp.StepIndex = -1;
        comp.ExpeditionSteps.Clear();
        comp.PuzzleKind = SpaceAnomalyStudyPuzzleKind.None;
        comp.PuzzleActive = false;
        comp.ActiveTarget = null;
        comp.MemoryPattern.Clear();
        comp.WireOrder.Clear();
        comp.CipherOptions.Clear();
    }

    private bool ValidateActiveTarget(EntityUid console, SpaceAnomalyStudyConsoleComponent comp, out EntityUid target)
    {
        target = default!;
        if (comp.ActiveTarget is not { } t || !Exists(t))
            return false;

        if (!_power.IsPowered(console) || !IsOnShuttleGrid(console) || !IsTargetInRange(console, comp, t))
            return false;

        target = t;
        return true;
    }

    private static bool ValidateWire(SpaceAnomalyStudyConsoleComponent comp, List<int> mapping)
    {
        for (var i = 0; i < 4; i++)
        {
            if (mapping[i] != comp.WireOrder[i])
                return false;
        }

        return true;
    }

    private (EntityUid, SpaceAnomalyStudyConsoleComponent)? FindLinkedConsole(EntityUid anomaly)
    {
        var query = EntityQueryEnumerator<SpaceAnomalyStudyConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ActiveTarget != anomaly || comp.StepIndex < 0)
                continue;

            var step = GetCurrentStep(comp);
            if (step == null || !IsEvaStep(step.Value))
                continue;

            return (uid, comp);
        }

        return null;
    }

    private static bool IsEvaStep(SpaceAnomalyStudyStepKind step) => step is
        SpaceAnomalyStudyStepKind.EvaFieldScan or
        SpaceAnomalyStudyStepKind.EvaCloseProbe or
        SpaceAnomalyStudyStepKind.EvaChemicalInject or
        SpaceAnomalyStudyStepKind.EvaStabilizerDeploy or
        SpaceAnomalyStudyStepKind.EvaSpectralScan;

    private static SpaceAnomalyStudyStepKind? GetCurrentStep(SpaceAnomalyStudyConsoleComponent comp)
    {
        if (comp.StepIndex < 0 || comp.StepIndex >= comp.ExpeditionSteps.Count)
            return null;

        return comp.ExpeditionSteps[comp.StepIndex];
    }

    private void UpdateUi(EntityUid uid, SpaceAnomalyStudyConsoleComponent comp)
    {
        var nearby = GetNearby(uid, comp);
        var currentStep = GetCurrentStep(comp) ?? SpaceAnomalyStudyStepKind.RemotePhaseLock;
        var hintLow = 0;
        var hintHigh = 0;
        var hintSecLow = 0;
        var hintSecHigh = 0;
        var waveforms = new List<string>();
        var cipherOptions = new List<string>();
        var cipherText = string.Empty;
        var memory = new List<int>();
        var wireOrder = new List<int>();

        if (comp.PuzzleActive)
        {
            switch (comp.PuzzleKind)
            {
                case SpaceAnomalyStudyPuzzleKind.PhaseLock:
                    hintLow = comp.PhaseTargetSum - comp.PhaseTolerance;
                    hintHigh = comp.PhaseTargetSum + comp.PhaseTolerance;
                    break;
                case SpaceAnomalyStudyPuzzleKind.Cipher:
                    cipherOptions = new List<string>(comp.CipherOptions);
                    if (comp.CipherAnswerIndex >= 0 && comp.CipherAnswerIndex < comp.CipherOptions.Count)
                        cipherText = SpaceAnomalyStudyPuzzles.EncodeCipher(comp.CipherOptions[comp.CipherAnswerIndex], comp.CipherShift);
                    break;
                case SpaceAnomalyStudyPuzzleKind.TimingPulse:
                    hintLow = comp.TimingZoneLow;
                    hintHigh = comp.TimingZoneHigh;
                    break;
                case SpaceAnomalyStudyPuzzleKind.WireRoute:
                    wireOrder = new List<int> { 0, 1, 2, 3 };
                    break;
                case SpaceAnomalyStudyPuzzleKind.MemoryGrid:
                    memory = new List<int>(comp.MemoryPattern);
                    break;
                case SpaceAnomalyStudyPuzzleKind.HarmonicSynthesis:
                    hintSecLow = comp.HarmonicProductTarget - comp.HarmonicProductTolerance;
                    hintSecHigh = comp.HarmonicProductTarget + comp.HarmonicProductTolerance;
                    break;
                case SpaceAnomalyStudyPuzzleKind.WaveformPick:
                    waveforms = SpaceAnomalyStudyPuzzles.BuildWaveformOptions();
                    break;
            }
        }

        var reward = 0;
        if (comp.ActiveTarget is { } target && TryComp<SpaceAnomalyComponent>(target, out var space))
            reward = space.StudyPointReward;

        var state = new SpaceAnomalyStudyBuiState
        {
            Nearby = nearby,
            ExpeditionPlan = new List<SpaceAnomalyStudyStepKind>(comp.ExpeditionSteps),
            CurrentStepIndex = comp.StepIndex,
            CurrentStep = currentStep,
            PuzzleKind = comp.PuzzleKind,
            PuzzleActive = comp.PuzzleActive,
            HintLow = hintLow,
            HintHigh = hintHigh,
            HintSecondaryLow = hintSecLow,
            HintSecondaryHigh = hintSecHigh,
            WaveformOptions = waveforms,
            CipherOptions = cipherOptions,
            CipherText = cipherText,
            CipherShift = comp.CipherShift,
            MemoryPattern = memory,
            WireOrder = wireOrder,
            TimingZoneLow = comp.TimingZoneLow,
            TimingZoneHigh = comp.TimingZoneHigh,
            RequiredReagent = comp.RequiredReagent,
            RequiredReagentUnits = comp.RequiredReagentUnits,
            PointReward = reward,
            ActiveTarget = comp.ActiveTarget == null ? null : GetNetEntity(comp.ActiveTarget.Value),
        };

        _ui.SetUiState(uid, SpaceAnomalyStudyUiKey.Key, state);
    }

    private List<SpaceAnomalyStudyEntry> GetNearby(EntityUid console, SpaceAnomalyStudyConsoleComponent comp)
    {
        var list = new List<SpaceAnomalyStudyEntry>();
        var consolePos = _transform.GetMapCoordinates(console);

        var query = EntityQueryEnumerator<SpaceAnomalyComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var space, out var xform))
        {
            var pos = _transform.GetMapCoordinates(uid, xform);
            if (pos.MapId != consolePos.MapId)
                continue;

            var dist = (pos.Position - consolePos.Position).Length();
            if (dist > comp.DetectionRange)
                continue;

            list.Add(new SpaceAnomalyStudyEntry(
                GetNetEntity(uid),
                MetaData(uid).EntityName,
                dist,
                space.Studied,
                space.StudyPointReward,
                Loc.GetString(space.TypeName)));
        }

        list.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        return list;
    }

    private static string GetStepLocale(SpaceAnomalyStudyStepKind step) => step switch
    {
        SpaceAnomalyStudyStepKind.RemotePhaseLock => "space-anomaly-study-stage-remote",
        SpaceAnomalyStudyStepKind.RemoteCipher => "space-anomaly-study-stage-cipher",
        SpaceAnomalyStudyStepKind.RemoteTimingPulse => "space-anomaly-study-stage-timing",
        SpaceAnomalyStudyStepKind.RemoteWireRoute => "space-anomaly-study-stage-wire",
        SpaceAnomalyStudyStepKind.RemoteMemoryGrid => "space-anomaly-study-stage-memory",
        SpaceAnomalyStudyStepKind.RemoteHarmonic => "space-anomaly-study-stage-harmonic",
        SpaceAnomalyStudyStepKind.EvaFieldScan => "space-anomaly-study-stage-eva-depart",
        SpaceAnomalyStudyStepKind.EvaChemicalInject => "space-anomaly-study-stage-chemical",
        SpaceAnomalyStudyStepKind.EvaCloseProbe => "space-anomaly-study-stage-close-probe",
        SpaceAnomalyStudyStepKind.EvaStabilizerDeploy => "space-anomaly-study-stage-stabilizer",
        SpaceAnomalyStudyStepKind.EvaSpectralScan => "space-anomaly-study-stage-spectral",
        SpaceAnomalyStudyStepKind.RemoteWaveformArchive => "space-anomaly-study-stage-waveform",
        _ => "space-anomaly-study-stage-idle",
    };

    private bool IsTargetInRange(EntityUid console, SpaceAnomalyStudyConsoleComponent comp, EntityUid target)
    {
        var consolePos = _transform.GetMapCoordinates(console);
        var targetPos = _transform.GetMapCoordinates(target);
        return (targetPos.Position - consolePos.Position).Length() <= comp.DetectionRange;
    }

    private bool IsOnShuttleGrid(EntityUid entity) => _transform.GetGrid(entity) != null;

    private bool IsOutsideShuttle(EntityUid user, EntityUid console)
    {
        var userGrid = _transform.GetGrid(user);
        var consoleGrid = _transform.GetGrid(console);
        return userGrid != consoleGrid;
    }

    private float GetDistance(EntityUid a, EntityUid b)
    {
        var posA = _transform.GetMapCoordinates(a);
        var posB = _transform.GetMapCoordinates(b);
        return (posB.Position - posA.Position).Length();
    }
}
