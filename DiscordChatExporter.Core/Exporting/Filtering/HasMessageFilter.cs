using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Parsing;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class HasMessageFilter(MessageContentMatchKind kind) : MessageFilter
{
    // Links can appear in the message content and in the text of components
    private static IEnumerable<LinkNode> ExtractLinks(Message message)
    {
        foreach (var link in MarkdownParser.ExtractLinks(message.Content))
            yield return link;

        foreach (var text in message.GetComponentTexts())
        foreach (var link in MarkdownParser.ExtractLinks(text))
            yield return link;
    }

    public override bool IsMatch(Message message) =>
        kind switch
        {
            MessageContentMatchKind.Link => ExtractLinks(message).Any(),
            MessageContentMatchKind.Embed => message.Embeds.Any(),
            MessageContentMatchKind.File => message.Attachments.Any(),
            MessageContentMatchKind.Video => message.Attachments.Any(file => file.IsVideo),
            MessageContentMatchKind.Image => message.Attachments.Any(file => file.IsImage),
            MessageContentMatchKind.Sound => message.Attachments.Any(file => file.IsAudio),
            MessageContentMatchKind.Pin => message.IsPinned,
            MessageContentMatchKind.Invite => ExtractLinks(message)
                .Select(l => l.Url)
                .Select(Invite.TryGetCodeFromUrl)
                .Any(c => !string.IsNullOrWhiteSpace(c)),
            _ => throw new InvalidOperationException(
                $"Unknown message content match kind '{kind}'."
            ),
        };
}
