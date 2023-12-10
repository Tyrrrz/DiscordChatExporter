using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Parsing;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal partial class PlainTextMarkdownVisitor(ExportContext context, StringBuilder buffer)
    : MarkdownVisitor
{
    protected override ValueTask VisitTextAsync(
        TextNode text,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(text.Text);
        return default;
    }

    protected override ValueTask VisitEmojiAsync(
        EmojiNode emoji,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(emoji.IsCustomEmoji ? $":{emoji.Name}:" : emoji.Name);

        return default;
    }

    protected override async ValueTask VisitMentionAsync(
        MentionNode mention,
        CancellationToken cancellationToken = default
    )
    {
        if (mention.Kind == MentionKind.Everyone)
        {
            buffer.Append("@everyone");
        }
        else if (mention.Kind == MentionKind.Here)
        {
            buffer.Append("@here");
        }
        else if (mention.Kind == MentionKind.User)
        {
            // User mentions are not always included in the message object,
            // which means they need to be populated on demand.
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/304
            if (mention.TargetId is not null)
                await context.PopulateMemberAsync(mention.TargetId.Value, cancellationToken);

            var member = mention.TargetId?.Pipe(context.TryGetMember);
            var displayName = member?.DisplayName ?? member?.User.DisplayName ?? "Unknown";

            buffer.Append($"@{displayName}");
        }
        else if (mention.Kind == MentionKind.Channel)
        {
            var channel = mention.TargetId?.Pipe(context.TryGetChannel);
            var name = channel?.Name ?? "deleted-channel";

            buffer.Append($"#{name}");

            // Voice channel marker
            if (channel?.IsVoice == true)
                buffer.Append(" [voice]");
        }
        else if (mention.Kind == MentionKind.Role)
        {
            var role = mention.TargetId?.Pipe(context.TryGetRole);
            var name = role?.Name ?? "deleted-role";

            buffer.Append($"@{name}");
        }
    }

    protected override ValueTask VisitTimestampAsync(
        TimestampNode timestamp,
        CancellationToken cancellationToken = default
    )
    {
        buffer.Append(
            timestamp.Instant is not null
                ? context.FormatDate(timestamp.Instant.Value, timestamp.Format ?? "g")
                : "Invalid date"
        );

        return default;
    }
}

internal partial class PlainTextMarkdownVisitor
{
    public static async ValueTask<string> FormatAsync(
        ExportContext context,
        string markdown,
        CancellationToken cancellationToken = default
    )
    {
        var nodes = MarkdownParser.ParseMinimal(markdown);

        var buffer = new StringBuilder();
        await new PlainTextMarkdownVisitor(context, buffer).VisitAsync(nodes, cancellationToken);

        return buffer.ToString();
    }
}
