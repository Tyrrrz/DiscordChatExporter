using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record YouTubeVideoEmbedProjection(string VideoId)
{
    public string Url => $"https://www.youtube.com/embed/{VideoId}";
}

public partial record YouTubeVideoEmbedProjection
{
    // Adapted from YoutubeExplode
    // https://github.com/Tyrrrz/YoutubeExplode/blob/bc700b631bd105d0be208a88116347034bdca88b/YoutubeExplode/Videos/VideoId.cs#L40-L62
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

        // Shorts URL
        // https://www.youtube.com/shorts/sKL1vjP0tIo
        var shortsMatch = Regex.Match(embedUrl, @"youtube\..+?/shorts/(.*?)(?:\?|&|/|$)").Groups[1].Value;
        if (!string.IsNullOrWhiteSpace(shortsMatch))
            return shortsMatch;

        return null;
    }

    public static YouTubeVideoEmbedProjection? TryResolve(Embed embed)
    {
        if (embed.Kind != EmbedKind.Video)
            return null;

        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        var videoId = TryParseVideoId(embed.Url);
        if (string.IsNullOrWhiteSpace(videoId))
            return null;

        return new YouTubeVideoEmbedProjection(videoId);
    }
}