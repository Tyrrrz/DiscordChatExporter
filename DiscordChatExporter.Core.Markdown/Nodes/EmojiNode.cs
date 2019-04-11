using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class EmojiNode : Node
    {
        public string Id { get; }

        public string Name { get; }

        public bool IsAnimated { get; }

        public bool IsCustomEmoji => !Id.IsNullOrWhiteSpace();

        public EmojiNode(string source, string id, string name, bool isAnimated)
            : base(source)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;
        }

        public EmojiNode(string source, string name)
            : this(source, null, name, false)
        {
        }

        public override string ToString() => $"<Emoji> {Name}";
    }
}