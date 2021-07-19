using System.Diagnostics.CodeAnalysis;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Markdown
{
    internal class EmojiNode : MarkdownNode
    {
        // Only present on custom emoji
        public string? Id { get; }

        // Name of custom emoji (e.g. LUL) or actual representation of standard emoji (e.g. 🙂)
        public string Name { get; }

        // Name of custom emoji (e.g. LUL) or name of standard emoji (e.g. slight_smile)
        public string Code => !string.IsNullOrWhiteSpace(Id)
            ? Name
            : EmojiIndex.TryGetCode(Name) ?? Name;

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

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"<Emoji> {Name}";
    }
}