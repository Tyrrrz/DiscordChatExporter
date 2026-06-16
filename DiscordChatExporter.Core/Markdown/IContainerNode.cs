using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

internal interface IContainerNode
{
    IReadOnlyList<MarkdownNode> Children { get; }
}
