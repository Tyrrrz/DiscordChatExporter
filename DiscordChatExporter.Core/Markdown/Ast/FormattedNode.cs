using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Ast
{
    internal enum TextFormatting
    {
        Bold,
        Italic,
        Underline,
        Strikethrough,
        Spoiler,
        Quote
    }

    internal class FormattedNode : MarkdownNode
    {
        public TextFormatting Formatting { get; }

        public IReadOnlyList<MarkdownNode> Children { get; }

        public FormattedNode(TextFormatting formatting, IReadOnlyList<MarkdownNode> children)
        {
            Formatting = formatting;
            Children = children;
        }

        public override string ToString() => $"<{Formatting}> (+{Children.Count})";
    }
}