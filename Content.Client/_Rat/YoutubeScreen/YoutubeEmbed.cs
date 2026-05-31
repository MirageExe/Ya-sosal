namespace Content.Client._Rat.YoutubeScreen;

/// <summary>
/// Loads YouTube via a local res:// wrapper so CEF sends a valid Referer (fixes YouTube error 153).
/// </summary>
public static class YoutubeEmbed
{
    private const string EmbedOrigin = "https://localhost";

    public static string ToResPageUrl(string videoId, bool autoplay)
    {
        var play = autoplay ? "1" : "0";
        return $"res://localhost/Webviews/youtube_screen.html?v={Uri.EscapeDataString(videoId)}&play={play}";
    }

    /// <summary>
    /// Fallback when res:// wrapper is unavailable.
    /// </summary>
    public static string ToDataUrl(string videoId, bool autoplay)
    {
        var play = autoplay ? "1" : "0";
        var origin = Uri.EscapeDataString(EmbedOrigin);
        var widgetReferrer = Uri.EscapeDataString($"{EmbedOrigin}/");
        var src =
            $"https://www.youtube.com/embed/{videoId}?autoplay={play}&mute=1&controls=0&modestbranding=1&rel=0&playsinline=1&fs=0&origin={origin}&widget_referrer={widgetReferrer}";

        var html = $$"""
            <!DOCTYPE html>
            <html><head>
            <meta charset="utf-8">
            <meta name="referrer" content="strict-origin-when-cross-origin">
            <style>html,body{margin:0;height:100%;background:#000}iframe{border:0;width:100%;height:100%}</style>
            </head><body>
            <iframe src="{{src}}" referrerpolicy="strict-origin-when-cross-origin" allow="autoplay; encrypted-media; picture-in-picture" allowfullscreen></iframe>
            </body></html>
            """;

        return "data:text/html;charset=utf-8," + Uri.EscapeDataString(html);
    }
}
