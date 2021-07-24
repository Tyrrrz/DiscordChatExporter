using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DiscordChatExporter.Core.Markdown
{
    internal class FormattingNode : MarkdownNode
    {
        public FormattingKind Kind { get; }

        public IReadOnlyList<MarkdownNode> Children { get; }

        public FormattingNode(FormattingKind kind, IReadOnlyList<MarkdownNode> children)
        {
            Kind = kind;
            Children = children;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var childrenFormatted = Children.Count == 1
                ? Children.Single().ToString()
                : "+" + Children.Count;

            return $"<{Kind}> ({childrenFormatted})";
        }
    }
}