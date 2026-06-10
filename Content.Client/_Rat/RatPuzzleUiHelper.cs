using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
using System.Numerics;

namespace Content.Client._Rat;

internal static class RatPuzzleUiHelper
{
    internal static readonly Color[] WireColors =
    {
        Color.FromHex("#E74C3C"),
        Color.FromHex("#3498DB"),
        Color.FromHex("#2ECC71"),
        Color.FromHex("#F1C40F"),
    };

    internal static readonly Color[] SequenceColors = WireColors;

    internal static string WireName(int index) => index switch
    {
        0 => Loc.GetString("rat-research-minigame-color-red"),
        1 => Loc.GetString("rat-research-minigame-color-blue"),
        2 => Loc.GetString("rat-research-minigame-color-green"),
        3 => Loc.GetString("rat-research-minigame-color-yellow"),
        _ => index.ToString(),
    };

    internal static PanelContainer MakeColorChip(int colorIndex, Vector2? size = null)
    {
        var panel = new PanelContainer { MinSize = size ?? new Vector2(48, 32) };
        panel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = WireColors[Math.Clamp(colorIndex, 0, 3)],
            BorderColor = Color.White.WithAlpha(0.35f),
            BorderThickness = new Thickness(1),
        };
        return panel;
    }

    internal static int ComputeBitMask(IReadOnlyList<Button> bitButtons)
    {
        var mask = 0;
        for (var i = 0; i < bitButtons.Count; i++)
        {
            if (bitButtons[i].Pressed)
                mask |= 1 << i;
        }

        return mask;
    }
}
