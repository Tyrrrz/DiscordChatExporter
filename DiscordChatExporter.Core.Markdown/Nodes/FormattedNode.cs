using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class FormattedNode : Node
    {
        public string Token { get; }

        public TextFormatting Formatting { get; }

        public IReadOnlyList<Node> Children { get; }

        public FormattedNode(string source, string token, TextFormatting formatting, IReadOnlyList<Node> children)
            : base(source)
        {
            Token = token;
            Formatting = formatting;
            Children = children;
        }

        public override string ToString() => $"<{Formatting}> ({Children.Count} direct children)";
    }
}