using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown
{
    public abstract class ContainerNode : Node
    {
        public IReadOnlyList<Node> Children { get; }

        protected ContainerNode(IReadOnlyList<Node> children)
        {
            Children = children;
        }
    }
}