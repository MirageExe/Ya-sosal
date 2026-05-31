using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Shared._Rat.YoutubeScreen;

/// <summary>
/// In-world screen that plays a YouTube embed for nearby clients (requires WebView/CEF on client).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class YoutubeScreenComponent : Component
{
    /// <summary>
    /// YouTube video id (11 characters) or empty when off.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string VideoId = "";

    [DataField, AutoNetworkedField]
    public bool Playing;

    /// <summary>
    /// How far away the embed is shown (meters).
    /// </summary>
    [DataField]
    public float MaxDistance = 48f;

    /// <summary>
    /// Pixel size of the browser surface on screen.
    /// </summary>
    [DataField]
    public Vector2i DisplaySize = new(640, 360);
}
