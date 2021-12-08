using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

public record PlainImageEmbedProjection(string Url)
{
    public static PlainImageEmbedProjection? TryResolve(Embed embed)
    {
        if (string.IsNullOrWhiteSpace(embed.Url))
            return null;

        // Has to be an embed without any data (except URL and image)
        if (!string.IsNullOrWhiteSpace(embed.Title) ||
            embed.Timestamp is not null ||
            embed.Author is not null ||
            !string.IsNullOrWhiteSpace(embed.Description) ||
            embed.Fields.Any() ||
            embed.Footer is not null)
        {
            return null;
        }

        // Has to be an image file
        var fileName = Regex.Match(embed.Url, @".+/([^?]*)").Groups[1].Value;
        if (string.IsNullOrWhiteSpace(fileName) || !FileFormat.IsImage(Path.GetExtension(fileName)))
            return null;

        return new PlainImageEmbedProjection(embed.Url);
    }
}