namespace DiscordChatExporter.Core.Markdown
{
    internal record MentionNode(string Id, MentionKind Kind) : MarkdownNode;
}