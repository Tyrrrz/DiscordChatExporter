using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record SpotifyTrackEmbedProjection(string TrackId)
{
    public string Url => $"https://open.spotify.com/embed/track/{TrackId}";
}

public partial record SpotifyTrackEmbedProjection
{
    private static string? TryParseTrackId(string embedUrl)
    {
        // https://open.spotify.com/track/1LHZMWefF9502NPfArRfvP?si=3efac6ce9be04f0a
        var trackId = Regex
            .Match(embedUrl, @"spotify\.com/track/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value;

        if (!string.IsNullOrWhiteSpace(trackId))
            return trackId;

        return null;
    }

    public static SpotifyTrackEmbedProjection? TryResolve(Embed embed)
    {
        if (embed.Kind != EmbedKind.Link)
            return null;

        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        var trackId = TryParseTrackId(embed.Url);
        if (string.IsNullOrWhiteSpace(trackId))
            return null;

        return new SpotifyTrackEmbedProjection(trackId);
    }
}
