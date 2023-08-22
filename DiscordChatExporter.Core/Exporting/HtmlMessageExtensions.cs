using System;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Embeds;

namespace DiscordChatExporter.Core.Exporting;

internal static class HtmlMessageExtensions
{
    // Message content is hidden if it's a link to an embedded media
    // https://github.com/Tyrrrz/DiscordChatExporter/issues/682
    public static bool IsContentHidden(this Message message)
    {
        if (message.Embeds.Count != 1)
            return false;

        var embed = message.Embeds[0];

        return string.Equals(message.Content.Trim(), embed.Url, StringComparison.OrdinalIgnoreCase)
            && embed.Kind is EmbedKind.Image or EmbedKind.Gifv;
    }
}
