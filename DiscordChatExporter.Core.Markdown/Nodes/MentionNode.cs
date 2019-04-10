namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class MentionNode : Node
    {
        public string Id { get; }

        public MentionType Type { get; }

        public MentionNode(string source, string id, MentionType type)
            : base(source)
        {
            Id = id;
            Type = type;
        }

        public override string ToString() => $"<{Type} mention> {Id}";
    }
}