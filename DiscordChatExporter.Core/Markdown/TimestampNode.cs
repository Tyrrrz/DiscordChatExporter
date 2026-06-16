using System;

namespace DiscordChatExporter.Core.Markdown;

// Null date means invalid timestamp
internal record TimestampNode(DateTimeOffset? Instant, string? Format) : MarkdownNode
{
    public static TimestampNode Invalid { get; } = new(null, null);
}
