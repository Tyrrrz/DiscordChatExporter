using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

internal record ListItemNode(IReadOnlyList<MarkdownNode> Children) : MarkdownNode, IContainerNode;
