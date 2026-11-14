using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#button
public record ButtonComponent(
    ButtonStyle Style,
    string? Label,
    Emoji? Emoji,
    string? Url,
    bool IsDisabled
) : MessageComponent(MessageComponentKind.Button)
{
    public static ButtonComponent Parse(JsonElement json)
    {
        var style =
            json.GetPropertyOrNull("style")?.GetInt32OrNull()?.Pipe(s => (ButtonStyle)s)
            ?? ButtonStyle.Secondary;

        var label = json.GetPropertyOrNull("label")?.GetNonWhiteSpaceStringOrNull();
        var emoji = json.GetPropertyOrNull("emoji")?.Pipe(Emoji.Parse);
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var isDisabled = json.GetPropertyOrNull("disabled")?.GetBooleanOrNull() ?? false;

        return new ButtonComponent(style, label, emoji, url, isDisabled);
    }
}
