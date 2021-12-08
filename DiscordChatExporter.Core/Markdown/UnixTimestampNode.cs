using System;

namespace DiscordChatExporter.Core.Markdown;

internal record UnixTimestampNode(DateTimeOffset Value) : MarkdownNode;