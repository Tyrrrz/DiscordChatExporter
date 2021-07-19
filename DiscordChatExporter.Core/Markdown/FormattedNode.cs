using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DiscordChatExporter.Core.Markdown
{
    internal class FormattedNode : MarkdownNode
    {
        public TextFormatting Formatting { get; }

        public IReadOnlyList<MarkdownNode> Children { get; }

        public FormattedNode(TextFormatting formatting, IReadOnlyList<MarkdownNode> children)
        {
            Formatting = formatting;
            Children = children;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"<{Formatting}> (+{Children.Count})";
    }
}