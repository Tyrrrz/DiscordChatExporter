namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record YouTubeVideoEmbedProjection(string VideoId)
{
    public string Url { get; } = $"https://www.youtube.com/watch?v={VideoId}";

    // Using hqdefault.jpg which is guaranteed to exist for all YouTube videos
    public string ThumbnailUrl { get; } = $"https://i.ytimg.com/vi/{VideoId}/hqdefault.jpg";

    public string GetThumbnailUrl(Embed embed) =>
        embed.Thumbnail?.ProxyUrl ?? embed.Thumbnail?.Url ?? ThumbnailUrl;
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
