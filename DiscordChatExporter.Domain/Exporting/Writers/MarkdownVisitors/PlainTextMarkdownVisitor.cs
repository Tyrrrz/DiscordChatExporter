using System.Linq;
using System.Text;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Markdown;
using DiscordChatExporter.Domain.Markdown.Ast;

namespace DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors
{
    internal partial class PlainTextMarkdownVisitor : MarkdownVisitor
    {
        private readonly RenderContext _context;
        private readonly StringBuilder _buffer;

        public PlainTextMarkdownVisitor(RenderContext context, StringBuilder buffer)
        {
            _context = context;
            _buffer = buffer;
        }

        public override MarkdownNode VisitText(TextNode text)
        {
            _buffer.Append(text.Text);
            return base.VisitText(text);
        }

        public override MarkdownNode VisitMention(MentionNode mention)
        {
            if (mention.Type == MentionType.User)
            {
                var user = _context.MentionableUsers.FirstOrDefault(u => u.Id == mention.Id) ??
                           User.CreateUnknownUser(mention.Id);

                _buffer.Append($"@{user.Name}");
            }
            else if (mention.Type == MentionType.Channel)
            {
                var channel = _context.MentionableChannels.FirstOrDefault(c => c.Id == mention.Id) ??
                              Channel.CreateDeletedChannel(mention.Id);

                _buffer.Append($"#{channel.Name}");
            }
            else if (mention.Type == MentionType.Role)
            {
                var role = _context.MentionableRoles.FirstOrDefault(r => r.Id == mention.Id) ??
                           Role.CreateDeletedRole(mention.Id);

                _buffer.Append($"@{role.Name}");
            }

            return base.VisitMention(mention);
        }

        public override MarkdownNode VisitEmoji(EmojiNode emoji)
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
        public static string Format(RenderContext context, string markdown)
        {
            var nodes = MarkdownParser.ParseMinimal(markdown);
            var buffer = new StringBuilder();

            new PlainTextMarkdownVisitor(context, buffer).Visit(nodes);

            return buffer.ToString();
        }
    }
}