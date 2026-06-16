using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Core.Markdown;

// Null ID means it's a meta mention or an invalid mention
internal record MentionNode(Snowflake? TargetId, MentionKind Kind) : MarkdownNode;
