namespace DiscordChatExporter.Core.Markdown
{
    public class EmojiNode : Node
    {
        public string Id { get; }

        public string Name { get; }

        public EmojiNode(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => $"<Emoji> {Name}";
    }
}