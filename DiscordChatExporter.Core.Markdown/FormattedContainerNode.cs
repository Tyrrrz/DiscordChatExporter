using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown
{
    public class FormattedContainerNode : ContainerNode
    {
        public TextFormatting Formatting { get; }

        public FormattedContainerNode(TextFormatting formatting, IReadOnlyList<Node> children)
            : base(children)
        {
            Formatting = formatting;
        }

        public override string ToString() => $"<{Formatting}> ({Children.Count} direct children)";
    }
}