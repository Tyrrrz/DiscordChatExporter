using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown;

internal record FormattingNode(FormattingKind Kind, IReadOnlyList<MarkdownNode> Children)
    : MarkdownNode,
        IContainerNode;
