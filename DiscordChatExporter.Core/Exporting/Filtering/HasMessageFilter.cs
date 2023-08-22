using System;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class HasMessageFilter : MessageFilter
{
    private readonly MessageContentMatchKind _kind;

    public HasMessageFilter(MessageContentMatchKind kind) => _kind = kind;

    public override bool IsMatch(Message message) =>
        _kind switch
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
                    $"Unknown message content match kind '{_kind}'."
                )
        };
}
