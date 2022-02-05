using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Core.Markdown;

internal record MentionNode(Snowflake Id, MentionKind Kind) : MarkdownNode;