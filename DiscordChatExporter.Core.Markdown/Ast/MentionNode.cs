namespace DiscordChatExporter.Core.Markdown.Ast
{
    public class MentionNode : Node
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