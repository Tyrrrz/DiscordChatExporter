using System;

namespace DiscordChatExporter.Core.Markdown;

// Null date means invalid timestamp
internal record UnixTimestampNode(DateTimeOffset? Date) : MarkdownNode;