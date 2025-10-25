using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using WebMarkupMin.Core;

namespace DiscordChatExporter.Core.Exporting;

internal partial class HtmlMessageWriter(Stream stream, ExportContext context, string themeName)
    : MessageWriter(stream, context)
{
    private readonly TextWriter _writer = new StreamWriter(stream);

    private readonly HtmlMinifier _minifier = new();
    private readonly List<Message> _messageGroup = [];

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
        await _writer.WriteLineAsync(
            Minify(
                await new PreambleTemplate { Context = Context, ThemeName = themeName }.RenderAsync(
                    cancellationToken
                )
            )
        );
    }

    private async ValueTask WriteMessageGroupAsync(
        IReadOnlyList<Message> messages,
        CancellationToken cancellationToken = default
    )
    {
        await _writer.WriteLineAsync(
            Minify(
                await new MessageGroupTemplate
                {
                    Context = Context,
                    Messages = messages,
                }.RenderAsync(cancellationToken)
            )
        );
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

    /// <summary>
    /// Returns the statically created regex that detects and captures the timestamp of a message in a channel HTML
    /// export.
    /// </summary>
    /// <returns>
    /// The regex that detects and captures the timestamp of a message in a Discord channel HTML export.
    /// </returns>
    [GeneratedRegex("<span class=chatlog__timestamp title=\"([^\"]*)\">")]
    private static partial Regex MessageDateRegex();

    /// <summary>
    /// Retrieves and returns the approximate timestamp of the last written message in the Discord channel that has
    /// been exported with the HtmlMessageWriter to the given file path as a Snowflake.
    /// This timestamp only has minute-level precision.
    /// </summary>
    /// <param name="filePath">
    /// The path of the Discord channel HTML export whose last message's timestamp should be returned.
    /// </param>
    /// <returns>
    /// The approximate timestamp of the last written message in the Discord channel HTML export under the given path
    /// as a Snowflake.
    /// Null, if the Discord channel HTML export doesn't include any message.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown if the file at the given path isn't a correctly formatted Discord channel HTML export.
    /// </exception>
    public static Snowflake? GetLastMessageDate(string filePath)
    {
        try
        {
            var fileContent = File.ReadAllText(filePath);
            var messageDateRegex = MessageDateRegex();

            var timestampMatches = messageDateRegex.Matches(fileContent);
            var timestampString = timestampMatches[^1].Groups[1].Value;
            var timestamp = DateTimeOffset.Parse(timestampString);
            return Snowflake.FromDate(timestamp, true);
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException or FormatException)
        {
            throw new FormatException(
                "The HTML file is not correctly formatted; the last message timestamp could not be retrieved."
            );
        }
    }
}
