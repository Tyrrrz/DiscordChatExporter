namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record YouTubeVideoEmbedProjection(string VideoId)
{
    public string Url => $"https://www.youtube.com/embed/{VideoId}";
}

public partial record YouTubeVideoEmbedProjection
{
    public static YouTubeVideoEmbedProjection? TryResolve(Embed embed)
    {
        if (embed.Kind != EmbedKind.Video)
            return null;

        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        var videoId = YoutubeExplode.Videos.VideoId.TryParse(embed.Url);
        if (videoId is null)
            return null;

        return new YouTubeVideoEmbedProjection(videoId);
    }
}
