using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Markdown;
using DiscordChatExporter.Core.Markdown.Parsing;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;

internal partial class PlainTextMarkdownVisitor : MarkdownVisitor
{
    private readonly ExportContext _context;
    private readonly StringBuilder _buffer;

    public PlainTextMarkdownVisitor(ExportContext context, StringBuilder buffer)
    {
        _context = context;
        _buffer = buffer;
    }

    protected override async ValueTask<MarkdownNode> VisitTextAsync(
        TextNode text,
        CancellationToken cancellationToken = default)
    {
        _buffer.Append(text.Text);
        return await base.VisitTextAsync(text, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitEmojiAsync(
        EmojiNode emoji,
        CancellationToken cancellationToken = default)
    {
        _buffer.Append(
            emoji.IsCustomEmoji
                ? $":{emoji.Name}:"
                : emoji.Name
        );

        return await base.VisitEmojiAsync(emoji, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitMentionAsync(
        MentionNode mention,
        CancellationToken cancellationToken = default)
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
            var member = mention.TargetId?.Pipe(_context.TryGetMember);
            var name = member?.User.Name ?? "Unknown";

            _buffer.Append($"@{name}");
        }
        else if (mention.Kind == MentionKind.Channel)
        {
            var channel = mention.TargetId?.Pipe(_context.TryGetChannel);
            var name = channel?.Name ?? "deleted-channel";

            _buffer.Append($"#{name}");

            // Voice channel marker
            if (channel?.SupportsVoice == true)
                _buffer.Append(" [voice]");
        }
        else if (mention.Kind == MentionKind.Role)
        {
            var role = mention.TargetId?.Pipe(_context.TryGetRole);
            var name = role?.Name ?? "deleted-role";

            _buffer.Append($"@{name}");
        }

        return await base.VisitMentionAsync(mention, cancellationToken);
    }

    protected override async ValueTask<MarkdownNode> VisitUnixTimestampAsync(
        UnixTimestampNode timestamp,
        CancellationToken cancellationToken = default)
    {
        _buffer.Append(
            timestamp.Date is not null
                ? _context.FormatDate(timestamp.Date.Value)
                : "Invalid date"
        );

        return await base.VisitUnixTimestampAsync(timestamp, cancellationToken);
    }
}

internal partial class PlainTextMarkdownVisitor
{
    public static async ValueTask<string> FormatAsync(
        ExportContext context,
        string markdown,
        CancellationToken cancellationToken = default)
    {
        var nodes = MarkdownParser.ParseMinimal(markdown);
        var buffer = new StringBuilder();

        await new PlainTextMarkdownVisitor(context, buffer)
            .VisitAsync(nodes, cancellationToken);

        return buffer.ToString();
    }
}