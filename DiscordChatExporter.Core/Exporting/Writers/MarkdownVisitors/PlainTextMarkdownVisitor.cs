using System.Text;
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

    protected override ValueTask<MarkdownNode> VisitTextAsync(TextNode text)
    {
        _buffer.Append(text.Text);
        return base.VisitTextAsync(text);
    }

    protected override ValueTask<MarkdownNode> VisitEmojiAsync(EmojiNode emoji)
    {
        _buffer.Append(
            emoji.IsCustomEmoji
                ? $":{emoji.Name}:"
                : emoji.Name
        );

        return base.VisitEmojiAsync(emoji);
    }

    protected override ValueTask<MarkdownNode> VisitMentionAsync(MentionNode mention)
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

        return base.VisitMentionAsync(mention);
    }

    protected override ValueTask<MarkdownNode> VisitUnixTimestampAsync(UnixTimestampNode timestamp)
    {
        _buffer.Append(
            timestamp.Date is not null
                ? _context.FormatDate(timestamp.Date.Value)
                : "Invalid date"
        );

        return base.VisitUnixTimestampAsync(timestamp);
    }
}

internal partial class PlainTextMarkdownVisitor
{
    public static async ValueTask<string> FormatAsync(ExportContext context, string markdown)
    {
        var nodes = MarkdownParser.ParseMinimal(markdown);
        var buffer = new StringBuilder();

        await new PlainTextMarkdownVisitor(context, buffer).VisitAsync(nodes);

        return buffer.ToString();
    }
}