using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using WebMarkupMin.Core;

namespace DiscordChatExporter.Core.Exporting;

internal class HtmlMessageWriter(Stream stream, ExportContext context, string themeName)
    : MessageWriter(stream, context)
{
    private readonly TextWriter _writer = new StreamWriter(stream);

    private readonly HtmlMinifier _minifier = new();
    private readonly List<Message> _messageGroup = [];

    private static string FormatMetadataAttribute(string name, object? value)
    {
        if (value is null)
            return "";

        var stringValue =
            value switch
            {
                bool boolean => boolean ? "true" : "false",
                IFormattable formattable => formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture
                ),
                _ => value.ToString(),
            } ?? "";

        return $" {name}=\"{WebUtility.HtmlEncode(stringValue)}\"";
    }

    private static string FormatMetadataAttributes(
        params (string Name, object? Value)[] attributes
    ) =>
        string.Concat(
            attributes.Select(attribute => FormatMetadataAttribute(attribute.Name, attribute.Value))
        );

    private string FormatMachineTimestamp(DateTimeOffset timestamp) =>
        Context.NormalizeDate(timestamp).ToString("O", CultureInfo.InvariantCulture);

    private string AddExportMetadata(string html)
    {
        if (!Context.Request.ShouldIncludeMachineMetadata)
            return html;

        const string marker = "<div class=\"chatlog\">";
        var replacement =
            "<div class=\"chatlog\""
            + FormatMetadataAttributes(
                ("data-machine-metadata-version", 1),
                ("data-guild-id", Context.Request.Guild.IsDirect ? null : Context.Request.Guild.Id),
                ("data-channel-id", Context.Request.Channel.Id),
                ("data-channel-type", Context.Request.Channel.Kind),
                ("data-parent-channel-id", Context.Request.Channel.Parent?.Id)
            )
            + ">";

        return html.Replace(marker, replacement, StringComparison.Ordinal);
    }

    private string AddMessageMetadata(string html, IReadOnlyList<Message> messages)
    {
        if (!Context.Request.ShouldIncludeMachineMetadata)
            return html;

        foreach (var message in messages)
        {
            var authorMember = Context.TryGetMember(message.Author.Id);
            var authorDisplayName = message.Author.IsBot
                ? message.Author.DisplayName
                : authorMember?.DisplayName ?? message.Author.DisplayName;

            var marker = $"data-message-id=\"{message.Id}\"";
            var replacement =
                marker
                + FormatMetadataAttributes(
                    ("data-message-type", message.Kind),
                    ("data-author-id", message.Author.Id),
                    ("data-author-name", message.Author.Name),
                    ("data-author-display-name", authorDisplayName),
                    ("data-author-is-bot", message.Author.IsBot),
                    ("data-timestamp", FormatMachineTimestamp(message.Timestamp)),
                    (
                        "data-edited-timestamp",
                        message.EditedTimestamp is { } editedTimestamp
                            ? FormatMachineTimestamp(editedTimestamp)
                            : null
                    ),
                    (
                        "data-call-ended-timestamp",
                        message.CallEndedTimestamp is { } callEndedTimestamp
                            ? FormatMachineTimestamp(callEndedTimestamp)
                            : null
                    ),
                    ("data-is-pinned", message.IsPinned),
                    ("data-is-forwarded", message.IsForwarded),
                    ("data-reference-type", message.Reference?.Kind),
                    ("data-reference-message-id", message.Reference?.MessageId),
                    ("data-reference-channel-id", message.Reference?.ChannelId),
                    ("data-reference-guild-id", message.Reference?.GuildId),
                    ("data-interaction-id", message.Interaction?.Id),
                    ("data-interaction-name", message.Interaction?.Name),
                    ("data-interaction-user-id", message.Interaction?.User.Id)
                );

            html = html.Replace(marker, replacement, StringComparison.Ordinal);
        }

        return html;
    }

    // Note: in reverse order, last message appears earlier than the first message
    private bool CanJoinGroup(Message message)
    {
        // If the group is empty, any message can join it
        if (_messageGroup.LastOrDefault() is not { } lastMessage)
            return true;

        // Reply-like messages cannot join existing groups because they need to appear first
        if (message.IsReplyLike)
            return false;

        // Grouping for system notifications
        if (message.IsSystemNotification)
        {
            // Can only be grouped with other system notifications
            if (!lastMessage.IsSystemNotification)
                return false;
        }
        // Grouping for normal messages
        else
        {
            // Can only be grouped with other normal messages
            if (lastMessage.IsSystemNotification)
                return false;

            // Messages must be within 7 minutes of each other
            if ((message.Timestamp - lastMessage.Timestamp).Duration().TotalMinutes > 7)
                return false;

            // Messages must be sent by the same author
            if (message.Author.Id != lastMessage.Author.Id)
                return false;

            // If the author changed their name after the last message, their new messages
            // cannot join the existing group.
            if (
                !string.Equals(
                    message.Author.FullName,
                    lastMessage.Author.FullName,
                    StringComparison.Ordinal
                )
            )
                return false;
        }

        return true;
    }

    // Use <!--wmm:ignore--> to preserve blocks of code inside the templates
    private string Minify(string html) => _minifier.Minify(html, false).MinifiedContent;

    public override async ValueTask WritePreambleAsync(
        CancellationToken cancellationToken = default
    )
    {
        var html = await new PreambleTemplate
        {
            Context = Context,
            ThemeName = themeName,
        }.RenderAsync(cancellationToken);

        await _writer.WriteLineAsync(Minify(AddExportMetadata(html)));
    }

    private async ValueTask WriteMessageGroupAsync(
        IReadOnlyList<Message> messages,
        CancellationToken cancellationToken = default
    )
    {
        var html = await new MessageGroupTemplate
        {
            Context = Context,
            Messages = messages,
        }.RenderAsync(cancellationToken);

        await _writer.WriteLineAsync(Minify(AddMessageMetadata(html, messages)));
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        await base.WriteMessageAsync(message, cancellationToken);

        // If the message can be grouped, buffer it for now
        if (CanJoinGroup(message))
        {
            _messageGroup.Add(message);
        }
        // Otherwise, flush the group and render messages
        else
        {
            await WriteMessageGroupAsync(_messageGroup, cancellationToken);

            _messageGroup.Clear();
            _messageGroup.Add(message);
        }
    }

    public override async ValueTask WritePostambleAsync(
        CancellationToken cancellationToken = default
    )
    {
        // Flush current message group
        if (_messageGroup.Any())
            await WriteMessageGroupAsync(_messageGroup, cancellationToken);

        await _writer.WriteLineAsync(
            Minify(
                await new PostambleTemplate
                {
                    Context = Context,
                    MessagesWritten = MessagesWritten,
                }.RenderAsync(cancellationToken)
            )
        );
    }

    public override async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
        await base.DisposeAsync();
    }
}
