using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

internal record HeadingNode(int Level, IReadOnlyList<MarkdownNode> Children)
    : MarkdownNode,
        IContainerNode;
