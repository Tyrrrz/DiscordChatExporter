using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

internal record LinkNode(
    string Url,
    IReadOnlyList<MarkdownNode> Children) : MarkdownNode
{
    public LinkNode(string url)
        : this(url, new[] { new TextNode(url) })
    {
    }
}