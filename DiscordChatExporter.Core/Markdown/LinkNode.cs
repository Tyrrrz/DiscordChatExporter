using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

// Named links can contain child nodes (e.g. [**bold URL**](https://test.com))
internal record LinkNode(string Url, IReadOnlyList<MarkdownNode> Children)
    : MarkdownNode,
        IContainerNode
{
    public LinkNode(string url)
        : this(url, [new TextNode(url)]) { }
}
