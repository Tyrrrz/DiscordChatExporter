using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Parsing;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal partial class PlainTextMarkdownVisitor : MarkdownVisitor
{
    private readonly ExportContext _context;
    private readonly StringBuilder _buffer;

    public PlainTextMarkdownVisitor(ExportContext context, StringBuilder buffer)
    {
        _context = context;
        _buffer = buffer;
    }

    protected override ValueTask VisitTextAsync(
        TextNode text,
        CancellationToken cancellationToken = default
    )
    {
        _buffer.Append(text.Text);
        return default;
    }

    protected override ValueTask VisitEmojiAsync(
        EmojiNode emoji,
        CancellationToken cancellationToken = default
    )
    {
        _buffer.Append(emoji.IsCustomEmoji ? $":{emoji.Name}:" : emoji.Name);

        return default;
    }

    protected override async ValueTask VisitMentionAsync(
        MentionNode mention,
        CancellationToken cancellationToken = default
    )
    {
        if (mention.Kind == MentionKind.Everyone)
        {
            _buffer.Append("@everyone");
        }
        else if (mention.Kind == MentionKind.Here)
        {
            _buffer.Append("@here");
        }
        else if (mention.Kind == MentionKind.User)
        {
            // User mentions are not always included in the message object,
            // which means they need to be populated on demand.
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/304
            if (mention.TargetId is not null)
                await _context.PopulateMemberAsync(mention.TargetId.Value, cancellationToken);

            var member = mention.TargetId?.Pipe(_context.TryGetMember);
            var displayName = member?.DisplayName ?? member?.User.DisplayName ?? "Unknown";

            _buffer.Append($"@{displayName}");
        }
        else if (mention.Kind == MentionKind.Channel)
        {
            var channel = mention.TargetId?.Pipe(_context.TryGetChannel);
            var name = channel?.Name ?? "deleted-channel";

            _buffer.Append($"#{name}");

            // Voice channel marker
            if (channel?.IsVoice == true)
                _buffer.Append(" [voice]");
        }
        else if (mention.Kind == MentionKind.Role)
        {
            var role = mention.TargetId?.Pipe(_context.TryGetRole);
            var name = role?.Name ?? "deleted-role";

            _buffer.Append($"@{name}");
        }
    }

    protected override ValueTask VisitTimestampAsync(
        TimestampNode timestamp,
        CancellationToken cancellationToken = default
    )
    {
        _buffer.Append(
            timestamp.Instant is not null
                ? _context.FormatDate(timestamp.Instant.Value, timestamp.Format ?? "g")
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
