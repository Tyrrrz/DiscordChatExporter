namespace DiscordChatExporter.Core.Markdown.Ast
{
    internal enum MentionType
    {
        Meta,
        User,
        Channel,
        Role
    }

    internal class MentionNode : MarkdownNode
    {
        public string Id { get; }

        public MentionType Type { get; }

        public MentionNode(string id, MentionType type)
        {
            Id = id;
            Type = type;
        }

        public override string ToString() => $"<{Type} mention> {Id}";
    }
}