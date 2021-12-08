using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record YouTubeVideoEmbedProjection(string VideoId)
{
    public string Url => $"https://www.youtube.com/embed/{VideoId}";
}

public partial record YouTubeVideoEmbedProjection
{
    // Adapted from YoutubeExplode
    // https://github.com/Tyrrrz/YoutubeExplode/blob/5be164be20019783913f76fcc98f18c65aebe9f0/YoutubeExplode/Videos/VideoId.cs#L34-L64
    private static string? TryParseVideoId(string embedUrl)
    {
        // Regular URL
        // https://www.youtube.com/watch?v=yIVRs6YSbOM
        var regularMatch = Regex.Match(embedUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)").Groups[1].Value;
        if (!string.IsNullOrWhiteSpace(regularMatch))
            return regularMatch;

        // Short URL
        // https://youtu.be/yIVRs6YSbOM
        var shortMatch = Regex.Match(embedUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)").Groups[1].Value;
        if (!string.IsNullOrWhiteSpace(shortMatch))
            return shortMatch;

        // Embed URL
        // https://www.youtube.com/embed/yIVRs6YSbOM
        var embedMatch = Regex.Match(embedUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)").Groups[1].Value;
        if (!string.IsNullOrWhiteSpace(embedMatch))
            return embedMatch;

        return null;
    }

    public static YouTubeVideoEmbedProjection? TryResolve(Embed embed)
    {
        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        var videoId = TryParseVideoId(embed.Url);
        if (string.IsNullOrWhiteSpace(videoId))
            return null;

        return new YouTubeVideoEmbedProjection(videoId);
    }
}