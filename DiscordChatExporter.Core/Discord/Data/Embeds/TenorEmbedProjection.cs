using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record TenorEmbedProjection(string Url)
{

    public static TenorEmbedProjection? TryResolve(Embed embed)
    {
        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        // Needs a the mp4 video URL linked in them embed json
        if (embed.Video is null || string.IsNullOrWhiteSpace(embed.Video.Url))
            return null;

        // Has to be a vaild Tenor URL
        if (!Regex.IsMatch(embed.Url, @"tenor\.com/view/[\w-]+$"))
            return null;

        return new TenorEmbedProjection(embed.Url);
    }
}