using System;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public partial record GifvEmbedProjection(string Url)
{
    public static GifvEmbedProjection? TryResolve(Embed embed)
    {
        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        if (embed.Video is null || string.IsNullOrWhiteSpace(embed.Video.Url))
            return null;

        if (!string.Equals(embed.Type, "gifv", StringComparison.OrdinalIgnoreCase))
            return null;

        return new GifvEmbedProjection(embed.Url);
    }
}