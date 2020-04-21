namespace DiscordChatExporter.Domain.Markdown.Ast
{
    internal class EmojiNode : MarkdownNode
    {
        public string? Id { get; }

        public string Name { get; }

        public bool IsAnimated { get; }

        public bool IsCustomEmoji => !string.IsNullOrWhiteSpace(Id);

        public EmojiNode(string? id, string name, bool isAnimated)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;
        }

        public EmojiNode(string name)
            : this(null, name, false)
        {
        }

        public override string ToString() => $"<Emoji> {Name}";
    }
}