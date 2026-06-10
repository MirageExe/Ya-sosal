using Content.Shared._Rat.SpaceAnomaly;
using Robust.Client.UserInterface;

namespace Content.Client._Rat.SpaceAnomaly;

public sealed class SpaceAnomalyStudyConsoleBui : BoundUserInterface
{
    private SpaceAnomalyStudyConsoleMenu? _menu;

    public SpaceAnomalyStudyConsoleBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<SpaceAnomalyStudyConsoleMenu>();
        _menu.OnRefresh += () => SendMessage(new SpaceAnomalyRefreshMessage());
        _menu.OnSelect += target => SendMessage(new SpaceAnomalySelectTargetMessage(target));
        _menu.OnBeginExpedition += () => SendMessage(new SpaceAnomalyBeginExpeditionMessage());
        _menu.OnSubmitPhaseLock += (a, b) => SendMessage(new SpaceAnomalySubmitPhaseLockMessage(a, b));
        _menu.OnSubmitCipher += i => SendMessage(new SpaceAnomalySubmitCipherMessage(i));
        _menu.OnSubmitTiming += p => SendMessage(new SpaceAnomalySubmitTimingMessage(p));
        _menu.OnSubmitWire += m => SendMessage(new SpaceAnomalySubmitWireMessage { Mapping = m });
        _menu.OnSubmitMemory += s => SendMessage(new SpaceAnomalySubmitMemoryMessage { Sequence = s });
        _menu.OnSubmitHarmonic += (s, v) => SendMessage(new SpaceAnomalySubmitHarmonicMessage(s, v));
        _menu.OnSubmitWaveform += i => SendMessage(new SpaceAnomalySubmitWaveformMessage(i));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is SpaceAnomalyStudyBuiState s)
            _menu?.UpdateState(s);
    }
}
