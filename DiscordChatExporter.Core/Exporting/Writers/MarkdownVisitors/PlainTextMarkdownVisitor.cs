using System.Text;
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

    protected override MarkdownNode VisitText(TextNode text)
    {
        _buffer.Append(text.Text);
        return base.VisitText(text);
    }

    protected override MarkdownNode VisitEmoji(EmojiNode emoji)
    {
        _buffer.Append(
            emoji.IsCustomEmoji
                ? $":{emoji.Name}:"
                : emoji.Name
        );

        return base.VisitEmoji(emoji);
    }

    protected override MarkdownNode VisitMention(MentionNode mention)
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
            var channel =mention.TargetId?.Pipe(_context.TryGetChannel);
            var name = channel?.Name ?? "deleted-channel";

            _buffer.Append($"#{name}");

            // Voice channel marker
            if (channel?.IsVoiceChannel == true)
                _buffer.Append(" [voice]");
        }
        else if (mention.Kind == MentionKind.Role)
        {
            var role = mention.TargetId?.Pipe(_context.TryGetRole);
            var name = role?.Name ?? "deleted-role";

            _buffer.Append($"@{name}");
        }

        return base.VisitMention(mention);
    }

    protected override MarkdownNode VisitUnixTimestamp(UnixTimestampNode timestamp)
    {
        _buffer.Append(
            timestamp.Date is not null
                ? _context.FormatDate(timestamp.Date.Value)
                : "Invalid date"
        );

        return base.VisitUnixTimestamp(timestamp);
    }
}

internal partial class PlainTextMarkdownVisitor
{
    public static string Format(ExportContext context, string markdown)
    {
        var nodes = MarkdownParser.ParseMinimal(markdown);
        var buffer = new StringBuilder();

        new PlainTextMarkdownVisitor(context, buffer).Visit(nodes);

        return buffer.ToString();
    }
}