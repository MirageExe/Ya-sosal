using Content.Shared._Rat.SpaceAnomaly;
using Robust.Shared.Random;

namespace Content.Server._Rat.SpaceAnomaly;

internal static class SpaceAnomalyStudyPuzzles
{
    private static readonly string[] CipherWords =
    {
        "DELTA",
        "SIGMA",
        "PHASE",
        "FLUX",
        "IONIC",
        "NEXUS",
    };

    private static readonly string[] ReagentPool =
    {
        "Water",
        "Plasma",
        "Epinephrine",
        "Blood",
        "Iron",
        "Copper",
        "Cryptobiolin",
    };

    private static readonly string[] WaveformNames =
    {
        "rat-research-waveform-alpha",
        "rat-research-waveform-beta",
        "rat-research-waveform-gamma",
        "rat-research-waveform-delta",
    };

    public static List<SpaceAnomalyStudyStepKind> BuildExpedition(IRobustRandom random, SpaceAnomalyBehaviorKind behavior)
    {
        var steps = new List<SpaceAnomalyStudyStepKind>(12);

        steps.Add(behavior switch
        {
            SpaceAnomalyBehaviorKind.Bluespace => SpaceAnomalyStudyStepKind.RemoteCipher,
            SpaceAnomalyBehaviorKind.Pyro => SpaceAnomalyStudyStepKind.EvaChemicalInject,
            SpaceAnomalyBehaviorKind.Electric => SpaceAnomalyStudyStepKind.RemoteWireRoute,
            SpaceAnomalyBehaviorKind.Ice => SpaceAnomalyStudyStepKind.EvaStabilizerDeploy,
            SpaceAnomalyBehaviorKind.Flesh => SpaceAnomalyStudyStepKind.EvaCloseProbe,
            SpaceAnomalyBehaviorKind.Shadow => SpaceAnomalyStudyStepKind.RemoteMemoryGrid,
            SpaceAnomalyBehaviorKind.Liquid => SpaceAnomalyStudyStepKind.EvaChemicalInject,
            SpaceAnomalyBehaviorKind.Flora => SpaceAnomalyStudyStepKind.EvaSpectralScan,
            _ => SpaceAnomalyStudyStepKind.RemotePhaseLock,
        });

        steps.Add(PickRemote(random));
        steps.Add(SpaceAnomalyStudyStepKind.EvaFieldScan);
        steps.Add(SpaceAnomalyStudyStepKind.EvaChemicalInject);
        steps.Add(PickRemote(random));
        steps.Add(SpaceAnomalyStudyStepKind.EvaCloseProbe);
        steps.Add(SpaceAnomalyStudyStepKind.EvaStabilizerDeploy);
        steps.Add(SpaceAnomalyStudyStepKind.EvaSpectralScan);
        steps.Add(PickRemote(random));
        steps.Add(SpaceAnomalyStudyStepKind.RemoteHarmonic);
        steps.Add(SpaceAnomalyStudyStepKind.RemoteWaveformArchive);

        return steps;
    }

    public static void SetupStep(SpaceAnomalyStudyConsoleComponent comp, SpaceAnomalyStudyStepKind step, IRobustRandom random)
    {
        comp.PuzzleActive = step is SpaceAnomalyStudyStepKind.RemotePhaseLock
            or SpaceAnomalyStudyStepKind.RemoteCipher
            or SpaceAnomalyStudyStepKind.RemoteTimingPulse
            or SpaceAnomalyStudyStepKind.RemoteWireRoute
            or SpaceAnomalyStudyStepKind.RemoteMemoryGrid
            or SpaceAnomalyStudyStepKind.RemoteHarmonic
            or SpaceAnomalyStudyStepKind.RemoteWaveformArchive;

        comp.PuzzleKind = step switch
        {
            SpaceAnomalyStudyStepKind.RemotePhaseLock => SpaceAnomalyStudyPuzzleKind.PhaseLock,
            SpaceAnomalyStudyStepKind.RemoteCipher => SpaceAnomalyStudyPuzzleKind.Cipher,
            SpaceAnomalyStudyStepKind.RemoteTimingPulse => SpaceAnomalyStudyPuzzleKind.TimingPulse,
            SpaceAnomalyStudyStepKind.RemoteWireRoute => SpaceAnomalyStudyPuzzleKind.WireRoute,
            SpaceAnomalyStudyStepKind.RemoteMemoryGrid => SpaceAnomalyStudyPuzzleKind.MemoryGrid,
            SpaceAnomalyStudyStepKind.RemoteHarmonic => SpaceAnomalyStudyPuzzleKind.HarmonicSynthesis,
            SpaceAnomalyStudyStepKind.RemoteWaveformArchive => SpaceAnomalyStudyPuzzleKind.WaveformPick,
            _ => SpaceAnomalyStudyPuzzleKind.None,
        };

        switch (step)
        {
            case SpaceAnomalyStudyStepKind.RemotePhaseLock:
                comp.PhaseTargetSum = random.Next(80, 141);
                break;
            case SpaceAnomalyStudyStepKind.RemoteCipher:
                SetupCipher(comp, random);
                break;
            case SpaceAnomalyStudyStepKind.RemoteTimingPulse:
                comp.TimingZoneLow = random.Next(35, 56);
                comp.TimingZoneHigh = comp.TimingZoneLow + random.Next(8, 16);
                break;
            case SpaceAnomalyStudyStepKind.RemoteWireRoute:
                SetupWire(comp, random);
                break;
            case SpaceAnomalyStudyStepKind.RemoteMemoryGrid:
                SetupMemory(comp, random);
                break;
            case SpaceAnomalyStudyStepKind.RemoteHarmonic:
                comp.HarmonicProductTarget = random.Next(900, 1901);
                break;
            case SpaceAnomalyStudyStepKind.RemoteWaveformArchive:
                comp.WaveformCorrectIndex = random.Next(0, WaveformNames.Length);
                break;
            case SpaceAnomalyStudyStepKind.EvaChemicalInject:
                comp.RequiredReagent = random.Pick(ReagentPool);
                comp.RequiredReagentUnits = random.Next(4, 9);
                break;
        }
    }

    public static void SetupCipher(SpaceAnomalyStudyConsoleComponent comp, IRobustRandom random)
    {
        comp.CipherShift = random.Next(1, 20);
        var wordIndex = random.Next(0, CipherWords.Length);
        comp.CipherOptions = BuildCipherOptions(wordIndex, random);
        comp.CipherAnswerIndex = comp.CipherOptions.IndexOf(CipherWords[wordIndex]);
    }

    public static string GetCipherWord(int index) => CipherWords[Math.Clamp(index, 0, CipherWords.Length - 1)];

    public static string EncodeCipher(string word, int shift)
    {
        var chars = new char[word.Length];
        for (var i = 0; i < word.Length; i++)
        {
            var c = word[i];
            if (c is >= 'A' and <= 'Z')
                chars[i] = (char) ('A' + (c - 'A' + shift) % 26);
            else
                chars[i] = c;
        }

        return new string(chars);
    }

    public static List<string> BuildCipherOptions(int correctIndex, IRobustRandom random)
    {
        var options = new List<string> { CipherWords[correctIndex] };
        while (options.Count < 4)
        {
            var pick = CipherWords[random.Next(CipherWords.Length)];
            if (!options.Contains(pick))
                options.Add(pick);
        }

        Shuffle(options, random);
        return options;
    }

    public static void SetupWire(SpaceAnomalyStudyConsoleComponent comp, IRobustRandom random)
    {
        comp.WireOrder.Clear();
        comp.WireOrder.AddRange(new[] { 0, 1, 2, 3 });
        Shuffle(comp.WireOrder, random);
    }

    public static void SetupMemory(SpaceAnomalyStudyConsoleComponent comp, IRobustRandom random)
    {
        comp.MemoryPattern.Clear();
        for (var i = 0; i < 4; i++)
            comp.MemoryPattern.Add(random.Next(0, 9));
    }

    public static void SetupBitRepair(SpaceAnomalyStudyConsoleComponent comp, IRobustRandom random)
    {
        comp.BitTarget = random.Next(0, 256);
        comp.BitMask = comp.BitTarget;
        for (var i = 0; i < 3; i++)
            comp.BitMask ^= 1 << random.Next(0, 8);
    }

    private static SpaceAnomalyStudyStepKind PickRemote(IRobustRandom random)
    {
        var pool = new[]
        {
            SpaceAnomalyStudyStepKind.RemotePhaseLock,
            SpaceAnomalyStudyStepKind.RemoteCipher,
            SpaceAnomalyStudyStepKind.RemoteTimingPulse,
            SpaceAnomalyStudyStepKind.RemoteWireRoute,
            SpaceAnomalyStudyStepKind.RemoteMemoryGrid,
        };

        return random.Pick(pool);
    }

    public static List<string> BuildWaveformOptions()
    {
        var list = new List<string>(WaveformNames.Length);
        foreach (var name in WaveformNames)
            list.Add(Loc.GetString(name));

        return list;
    }

    private static void Shuffle<T>(List<T> list, IRobustRandom random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
