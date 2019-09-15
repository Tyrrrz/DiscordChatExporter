using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class FormattedNode : Node
    {
        public TextFormatting Formatting { get; }

        public IReadOnlyList<Node> Children { get; }

        public FormattedNode(TextFormatting formatting, IReadOnlyList<Node> children)
        {
            Formatting = formatting;
            Children = children;
        }

        public override string ToString() => $"<{Formatting}> ({Children.Count} direct children)";
    }
}