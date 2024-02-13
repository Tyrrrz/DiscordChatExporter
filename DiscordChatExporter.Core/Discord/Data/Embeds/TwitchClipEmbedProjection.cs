using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record TwitchClipEmbedProjection(string ClipId)
{
    public string Url => $"https://clips.twitch.tv/embed?clip={ClipId}&parent=localhost";
}

public partial record TwitchClipEmbedProjection
{
    private static string? TryParseClipId(string embedUrl)
    {
        // https://clips.twitch.tv/SpookyTenuousPidgeonPanicVis
        {
            var clipId = Regex
                .Match(embedUrl, @"clips\.twitch\.tv/(.*?)(?:\?|&|/|$)")
                .Groups[1]
                .Value;

            if (!string.IsNullOrWhiteSpace(clipId))
                return clipId;
        }

        // https://twitch.tv/clip/SpookyTenuousPidgeonPanicVis
        {
            var clipId = Regex
                .Match(embedUrl, @"twitch\.tv/clip/(.*?)(?:\?|&|/|$)")
                .Groups[1]
                .Value;

            if (!string.IsNullOrWhiteSpace(clipId))
                return clipId;
        }

        return null;
    }

    public static TwitchClipEmbedProjection? TryResolve(Embed embed)
    {
        if (embed.Kind != EmbedKind.Video)
            return null;

        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        var clipId = TryParseClipId(embed.Url);
        if (string.IsNullOrWhiteSpace(clipId))
            return null;

        return new TwitchClipEmbedProjection(clipId);
    }
}
