using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

internal record ListNode(IReadOnlyList<ListItemNode> Items) : MarkdownNode;
