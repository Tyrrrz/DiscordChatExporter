using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Markdown.Parsing;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class HasMessageFilter(MessageContentMatchKind kind) : MessageFilter
{
    // Links can appear in the message content, in the text of components, and in link buttons
    private static IEnumerable<string> ExtractLinkUrls(Message message)
    {
        foreach (var link in MarkdownParser.ExtractLinks(message.Content))
            yield return link.Url;

        foreach (var text in message.GetComponentTexts())
        foreach (var link in MarkdownParser.ExtractLinks(text))
            yield return link.Url;

        foreach (var url in message.GetComponentButtonUrls())
            yield return url;
    }

    public override bool IsMatch(Message message) =>
        kind switch
        {
            MessageContentMatchKind.Link => ExtractLinkUrls(message).Any(),
            MessageContentMatchKind.Embed => message.Embeds.Any(),
            MessageContentMatchKind.File => message.Attachments.Any()
                || message.GetComponentMedia().Any(),
            MessageContentMatchKind.Video => message.Attachments.Any(file => file.IsVideo)
                || message.GetComponentMedia().Any(media => media.IsVideo),
            MessageContentMatchKind.Image => message.Attachments.Any(file => file.IsImage)
                || message.GetComponentMedia().Any(media => media.IsImage),
            MessageContentMatchKind.Sound => message.Attachments.Any(file => file.IsAudio)
                || message.GetComponentMedia().Any(media => media.IsAudio),
            MessageContentMatchKind.Pin => message.IsPinned,
            MessageContentMatchKind.Invite => ExtractLinkUrls(message)
                .Select(Invite.TryGetCodeFromUrl)
                .Any(c => !string.IsNullOrWhiteSpace(c)),
            _ => throw new InvalidOperationException(
                $"Unknown message content match kind '{kind}'."
            ),
        };
}
