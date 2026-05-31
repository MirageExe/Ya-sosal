namespace Content.Shared._Rat.YoutubeScreen;

/// <summary>
/// Parses YouTube URLs/ids without regex (Content.Shared sandbox).
/// </summary>
public static class YoutubeUrl
{
    public static string? ParseVideoId(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();

        if (input.Length == 11 && IsVideoId(input))
            return input;

        if (TryAfter(input, "youtu.be/", out var id))
            return id;

        if (TryAfter(input, "youtube.com/embed/", out id))
            return id;

        if (TryAfter(input, "youtube.com/shorts/", out id))
            return id;

        if (TryAfter(input, "watch?v=", out id))
            return id;

        return null;
    }

    public static string ToEmbedUrl(string videoId, bool autoplay)
    {
        var autoplayFlag = autoplay ? "1" : "0";
        return $"https://www.youtube.com/embed/{videoId}?autoplay={autoplayFlag}&mute=1&controls=0&modestbranding=1&rel=0&playsinline=1&fs=0";
    }

    private static bool TryAfter(string input, string marker, out string? videoId)
    {
        videoId = null;
        var idx = input.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return false;

        return TryReadVideoId(input, idx + marker.Length, out videoId);
    }

    private static bool TryReadVideoId(string input, int start, out string? videoId)
    {
        videoId = null;
        if (start + 11 > input.Length)
            return false;

        var candidate = input.Substring(start, 11);
        if (!IsVideoId(candidate))
            return false;

        if (start + 11 < input.Length)
        {
            var next = input[start + 11];
            if (char.IsLetterOrDigit(next) || next == '_' || next == '-')
                return false;
        }

        videoId = candidate;
        return true;
    }

    private static bool IsVideoId(string value)
    {
        if (value.Length != 11)
            return false;

        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c) || c is '_' or '-')
                continue;

            return false;
        }

        return true;
    }
}
