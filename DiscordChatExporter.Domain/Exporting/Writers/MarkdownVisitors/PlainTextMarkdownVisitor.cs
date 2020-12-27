using System.Text;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Markdown;
using DiscordChatExporter.Domain.Markdown.Ast;

namespace DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors
{
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

        protected override MarkdownNode VisitMention(MentionNode mention)
        {
            if (mention.Type == MentionType.Meta)
            {
                _buffer.Append($"@{mention.Id}");
            }
            else if (mention.Type == MentionType.User)
            {
                var member = _context.TryGetMember(Snowflake.Parse(mention.Id));
                var name = member?.User.Name ?? "Unknown";

                _buffer.Append($"@{name}");
            }
            else if (mention.Type == MentionType.Channel)
            {
                var channel = _context.TryGetChannel(Snowflake.Parse(mention.Id));
                var name = channel?.Name ?? "deleted-channel";

                _buffer.Append($"#{name}");
            }
            else if (mention.Type == MentionType.Role)
            {
                var role = _context.TryGetRole(Snowflake.Parse(mention.Id));
                var name = role?.Name ?? "deleted-role";

                _buffer.Append($"@{name}");
            }

            return base.VisitMention(mention);
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
}