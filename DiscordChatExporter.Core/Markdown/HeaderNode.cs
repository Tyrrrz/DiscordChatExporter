using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

internal record HeaderNode(
    int Level,
    IReadOnlyList<MarkdownNode> Children
) : MarkdownNode, IContainerNode;