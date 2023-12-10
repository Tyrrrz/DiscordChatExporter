using System;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class HasMessageFilter(MessageContentMatchKind kind) : MessageFilter
{
    public override bool IsMatch(Message message) =>
        kind switch
        {
            MessageContentMatchKind.Link
                => Regex.IsMatch(message.Content, "https?://\\S*[^\\.,:;\"\'\\s]"),
            MessageContentMatchKind.Embed => message.Embeds.Any(),
            MessageContentMatchKind.File => message.Attachments.Any(),
            MessageContentMatchKind.Video => message.Attachments.Any(file => file.IsVideo),
            MessageContentMatchKind.Image => message.Attachments.Any(file => file.IsImage),
            MessageContentMatchKind.Sound => message.Attachments.Any(file => file.IsAudio),
            MessageContentMatchKind.Pin => message.IsPinned,
            _
                => throw new InvalidOperationException(
                    $"Unknown message content match kind '{kind}'."
                )
        };
}
