namespace DiscordChatExporter.Core.Markdown
{
    public class MentionNode : Node
    {
        public string Id { get; }

        public MentionType Type { get; }

        public MentionNode(string lexeme, string id, MentionType type)
            : base(lexeme)
        {
            Id = id;
            Type = type;
        }

        public override string ToString() => $"<{Type} mention> {Id}";
    }
}