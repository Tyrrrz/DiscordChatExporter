using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Markdown
{
    internal record EmojiNode(
        // Only present on custom emoji
        string? Id,
        // Name of custom emoji (e.g. LUL) or actual representation of standard emoji (e.g. 🙂)
        string Name,
        bool IsAnimated) : MarkdownNode
    {
        // Name of custom emoji (e.g. LUL) or name of standard emoji (e.g. slight_smile)
        public string Code => !string.IsNullOrWhiteSpace(Id)
            ? Name
            : EmojiIndex.TryGetCode(Name) ?? Name;

        public bool IsCustomEmoji => !string.IsNullOrWhiteSpace(Id);

        public EmojiNode(string name)
            : this(null, name, false)
        {
        }
    }
}