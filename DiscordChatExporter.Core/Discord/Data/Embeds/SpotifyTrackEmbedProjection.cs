using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    public partial class SpotifyTrackEmbedProjection
    {
        public string TrackId { get; }

        public string Url => $"https://open.spotify.com/embed/track/{TrackId}";

        public SpotifyTrackEmbedProjection(string trackId) => TrackId = trackId;

        [ExcludeFromCodeCoverage]
        public override string ToString() => Url;
    }

    public partial class SpotifyTrackEmbedProjection
    {
        private static string? TryParseTrackId(string embedUrl)
        {
            // https://open.spotify.com/track/1LHZMWefF9502NPfArRfvP?si=3efac6ce9be04f0a
            var trackId = Regex.Match(embedUrl, @"spotify\.com/track/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(trackId))
                return trackId;

            return null;
        }

        public static SpotifyTrackEmbedProjection? TryResolve(Embed embed)
        {
            if (string.IsNullOrWhiteSpace(embed.Url))
                return null;

            var trackId = TryParseTrackId(embed.Url);
            if (string.IsNullOrWhiteSpace(trackId))
                return null;

            return new SpotifyTrackEmbedProjection(trackId);
        }
    }
}