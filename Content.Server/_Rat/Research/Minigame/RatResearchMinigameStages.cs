using Content.Server._Rat.SpaceAnomaly;
using Content.Shared._Rat.Research.Minigame;

using Robust.Shared.Random;



namespace Content.Server._Rat.Research.Minigame;



internal static class RatResearchMinigameStages

{

    public static int GetStageCount(RatResearchMinigameType type) => type switch

    {

        RatResearchMinigameType.FrequencyTune => 3,

        RatResearchMinigameType.SequenceMatch => 3,

        RatResearchMinigameType.PhaseLock => 2,

        RatResearchMinigameType.HarmonicBalance => 2,

        RatResearchMinigameType.WaveformPick => 2,

        RatResearchMinigameType.CipherDecrypt => 2,

        RatResearchMinigameType.TimingPulse => 2,

        RatResearchMinigameType.WireRouting => 2,

        RatResearchMinigameType.MemoryGrid => 2,

        RatResearchMinigameType.BitRepair => 2,

        _ => 1,

    };



    public static void Setup(RatResearchMinigameConsoleComponent comp, IRobustRandom random)

    {

        comp.PuzzleStage = 0;

        comp.PuzzleStageCount = GetStageCount(comp.ActiveType);



        switch (comp.ActiveType)

        {

            case RatResearchMinigameType.FrequencyTune:

                comp.PuzzleTarget = random.Next(20, 81);

                comp.TuneBandTarget = comp.PuzzleTarget switch

                {

                    < 34 => 0,

                    < 67 => 1,

                    _ => 2,

                };

                comp.PuzzleSecondaryTarget = random.Next(10, 91);

                break;

            case RatResearchMinigameType.SequenceMatch:

                comp.PuzzleSequence.Clear();

                for (var i = 0; i < 4; i++)

                    comp.PuzzleSequence.Add(random.Next(0, 4));

                comp.SequenceTransformMode = 0;

                break;

            case RatResearchMinigameType.PhaseLock:

                comp.PuzzleTarget = random.Next(80, 141);

                comp.PuzzleSecondaryTarget = random.Next(8, 36);

                break;

            case RatResearchMinigameType.HarmonicBalance:

                comp.PuzzleSecondaryTarget = random.Next(900, 1901);

                comp.PuzzleTarget = SolveHarmonicStability(comp.PuzzleSecondaryTarget);

                break;

            case RatResearchMinigameType.WaveformPick:

                comp.WaveformCorrectIndex = random.Next(0, 4);

                comp.WaveformNoiseIndices.Clear();

                while (comp.WaveformNoiseIndices.Count < 2)

                {

                    var pick = random.Next(0, 4);

                    if (pick != comp.WaveformCorrectIndex && !comp.WaveformNoiseIndices.Contains(pick))

                        comp.WaveformNoiseIndices.Add(pick);

                }



                comp.WaveformNoiseIndices.Sort();

                break;

            case RatResearchMinigameType.CipherDecrypt:

                SetupCipher(comp, random);

                comp.CipherShiftOptions.Clear();

                comp.CipherShiftOptions.Add(comp.CipherShift);

                while (comp.CipherShiftOptions.Count < 3)

                {

                    var alt = random.Next(1, 20);

                    if (!comp.CipherShiftOptions.Contains(alt))

                        comp.CipherShiftOptions.Add(alt);

                }



                Shuffle(comp.CipherShiftOptions, random);

                break;

            case RatResearchMinigameType.TimingPulse:

                comp.TimingZoneLow = random.Next(35, 56);

                comp.TimingZoneHigh = comp.TimingZoneLow + random.Next(8, 16);

                break;

            case RatResearchMinigameType.WireRouting:

                ShuffleWire(comp, random);

                break;

            case RatResearchMinigameType.MemoryGrid:

                comp.MemoryPattern.Clear();

                for (var i = 0; i < 4; i++)

                    comp.MemoryPattern.Add(random.Next(0, 9));

                comp.SequenceTransformMode = 0;

                break;

            case RatResearchMinigameType.BitRepair:

                comp.BitTarget = random.Next(0, 256);

                comp.BitMask = comp.BitTarget;

                for (var i = 0; i < 3; i++)

                    comp.BitMask ^= 1 << random.Next(0, 8);

                comp.PuzzleTarget = PopCount(comp.BitMask ^ comp.BitTarget);

                break;

        }

    }



    public static void AdvanceStage(RatResearchMinigameConsoleComponent comp)

    {

        comp.PuzzleStage++;



        switch (comp.ActiveType)

        {

            case RatResearchMinigameType.SequenceMatch:

                comp.SequenceTransformMode = comp.PuzzleStage switch

                {

                    1 => 1,

                    2 => 2,

                    _ => 0,

                };

                break;

            case RatResearchMinigameType.MemoryGrid:

                comp.SequenceTransformMode = comp.PuzzleStage >= 1 ? 1 : 0;

                break;

        }

    }



    public static int GetTuneBandHintHigh(int target) => target switch

    {

        0 => 33,

        1 => 66,

        _ => 100,

    };



    public static int GetTuneBandHintLow(int target) => target switch

    {

        0 => 0,

        1 => 34,

        _ => 67,

    };



    public static List<int> TransformSequence(List<int> source, int mode)

    {

        var list = new List<int>(source);

        switch (mode)

        {

            case 1:

                list.Reverse();

                break;

            case 2:

                if (list.Count > 0)

                {

                    var first = list[0];

                    list.RemoveAt(0);

                    list.Add(first);

                }



                break;

        }



        return list;

    }



    public static bool SequencesEqual(List<int> a, List<int> b)

    {

        if (a.Count != b.Count)

            return false;



        for (var i = 0; i < a.Count; i++)

        {

            if (a[i] != b[i])

                return false;

        }



        return true;

    }



    private static void SetupCipher(RatResearchMinigameConsoleComponent comp, IRobustRandom random)

    {

        var wordIndex = random.Next(0, 6);

        comp.CipherShift = random.Next(1, 20);

        comp.CipherOptions = SpaceAnomalyStudyPuzzles.BuildCipherOptions(wordIndex, random);

        var word = SpaceAnomalyStudyPuzzles.GetCipherWord(wordIndex);

        comp.CipherAnswerIndex = comp.CipherOptions.IndexOf(word);

    }



    private static void ShuffleWire(RatResearchMinigameConsoleComponent comp, IRobustRandom random)

    {

        comp.WireOrder.Clear();

        comp.WireOrder.AddRange(new[] { 0, 1, 2, 3 });

        Shuffle(comp.WireOrder, random);

    }



    private static int SolveHarmonicStability(int product)

    {

        for (var s = 1; s < 100; s++)

        {

            if (s * (100 - s) == product)

                return s;

        }



        return 50;

    }



    private static int PopCount(int value)

    {

        var count = 0;

        while (value != 0)

        {

            count += value & 1;

            value >>= 1;

        }



        return count;

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


