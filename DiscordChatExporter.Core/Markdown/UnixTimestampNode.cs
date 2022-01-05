using System;

namespace DiscordChatExporter.Core.Markdown;

// Null means invalid date
internal record UnixTimestampNode(DateTimeOffset? Date) : MarkdownNode;