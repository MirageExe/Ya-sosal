using Content.Shared._Rat.Research.Minigame;
using Robust.Client.UserInterface;

namespace Content.Client._Rat.Research.Minigame;

public sealed class RatResearchMinigameBui : BoundUserInterface
{
    private RatResearchMinigameMenu? _menu;

    public RatResearchMinigameBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<RatResearchMinigameMenu>();
        _menu.OnStartTune += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.FrequencyTune));
        _menu.OnStartSequence += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.SequenceMatch));
        _menu.OnStartPhase += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.PhaseLock));
        _menu.OnStartHarmonic += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.HarmonicBalance));
        _menu.OnStartWaveform += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.WaveformPick));
        _menu.OnStartCipher += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.CipherDecrypt));
        _menu.OnStartTiming += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.TimingPulse));
        _menu.OnStartWire += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.WireRouting));
        _menu.OnStartMemory += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.MemoryGrid));
        _menu.OnStartBitRepair += () => SendMessage(new RatResearchStartMinigameMessage(RatResearchMinigameType.BitRepair));
        _menu.OnSubmitTuneBand += band => SendMessage(new RatResearchSubmitTuneBandMessage(band));
        _menu.OnSubmitTune += (a, b, c) => SendMessage(new RatResearchSubmitTuneMessage(a, b, c));
        _menu.OnSubmitTuneFinalize += (a, b, c) => SendMessage(new RatResearchSubmitTuneFinalizeMessage(a, b, c));
        _menu.OnSubmitSequence += seq => SendMessage(new RatResearchSubmitSequenceMessage { Sequence = seq });
        _menu.OnSubmitPhase += (a, b) => SendMessage(new RatResearchSubmitPhaseLockMessage(a, b));
        _menu.OnSubmitHarmonic += (s, v) => SendMessage(new RatResearchSubmitHarmonicMessage(s, v));
        _menu.OnSubmitWaveformFilter += indices => SendMessage(new RatResearchSubmitWaveformFilterMessage { NoiseIndices = indices });
        _menu.OnSubmitWaveform += i => SendMessage(new RatResearchSubmitWaveformMessage(i));
        _menu.OnSubmitCipherShift += shift => SendMessage(new RatResearchSubmitCipherShiftMessage(shift));
        _menu.OnSubmitCipher += i => SendMessage(new RatResearchSubmitCipherMessage(i));
        _menu.OnSubmitTiming += p => SendMessage(new RatResearchSubmitTimingMessage(p));
        _menu.OnSubmitWirePort += port => SendMessage(new RatResearchSubmitWirePortMessage(port));
        _menu.OnSubmitWire += m => SendMessage(new RatResearchSubmitWireMessage { Mapping = m });
        _menu.OnSubmitMemory += s => SendMessage(new RatResearchSubmitMemoryMessage { Sequence = s });
        _menu.OnSubmitBitCount += count => SendMessage(new RatResearchSubmitBitCountMessage(count));
        _menu.OnSubmitBitRepair += m => SendMessage(new RatResearchSubmitBitRepairMessage(m));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is RatResearchMinigameBuiState s)
            _menu?.UpdateState(s);
    }
}
