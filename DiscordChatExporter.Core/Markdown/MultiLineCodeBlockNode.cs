namespace DiscordChatExporter.Core.Markdown;

internal record MultiLineCodeBlockNode(string Language, string Code) : MarkdownNode;
