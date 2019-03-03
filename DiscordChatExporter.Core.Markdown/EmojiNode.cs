using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Markdown
{
    public class EmojiNode : Node
    {
        public string Id { get; }

        public string Name { get; }

        public bool IsAnimated { get; }

        public bool IsCustomEmoji => Id.IsNotBlank();

        public EmojiNode(string lexeme, string id, string name, bool isAnimated)
            : base(lexeme)
        {
            Id = id;
            Name = name;
            IsAnimated = isAnimated;
        }

        public EmojiNode(string lexeme, string name)
            : this(lexeme, null, name, false)
        {
        }

        public override string ToString() => $"<Emoji> {Name}";
    }
}