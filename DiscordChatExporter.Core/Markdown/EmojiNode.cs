using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Markdown;

internal record EmojiNode(
    // Only present on custom emoji
    Snowflake? Id,
    // Name of a custom emoji (e.g. LUL) or actual representation of a standard emoji (e.g. 🙂)
    string Name,
    bool IsAnimated
) : MarkdownNode
{
    public bool IsCustomEmoji => Id is not null;

    // Name of a custom emoji (e.g. LUL) or name of a standard emoji (e.g. slight_smile)
    public string Code => IsCustomEmoji ? Name : EmojiIndex.TryGetCode(Name) ?? Name;

    public EmojiNode(string name)
        : this(null, name, false) { }
}
