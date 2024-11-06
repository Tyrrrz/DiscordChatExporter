using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Markdown;

internal record EmojiNode(
    // Only present on custom emoji
    Snowflake? Id,
    // Name of a custom emoji (e.g. LUL) or actual representation of a standard emoji (e.g. 🙂)
    string Name,
    bool IsAnimated
) : MarkdownNode
{
    // This coupling is unsound from the domain-design perspective, but it helps us reuse
    // some code for now. We can refactor this later, if the coupling becomes a problem.
    private readonly Emoji _emoji = new(Id, Name, IsAnimated);

    public EmojiNode(string name)
        : this(null, name, false) { }

    public bool IsCustomEmoji => _emoji.IsCustomEmoji;

    // Name of a custom emoji (e.g. LUL) or name of a standard emoji (e.g. slight_smile)
    public string Code => _emoji.Code;

    public string ImageUrl => _emoji.ImageUrl;
}
