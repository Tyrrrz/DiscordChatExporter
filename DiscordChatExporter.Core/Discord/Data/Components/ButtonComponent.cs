using System;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#button
public partial record ButtonComponent(
    ButtonStyle Style,
    string? Label,
    Emoji? Emoji,
    string? Url,
    string? CustomId,
    Snowflake? SkuId,
    bool IsDisabled
)
{
    public bool IsUrlButton => !string.IsNullOrWhiteSpace(Url);
}

public partial record ButtonComponent
{
    public static ButtonComponent Parse(JsonElement json)
    {
        var style =
            json.GetPropertyOrNull("style")
                ?.GetInt32OrNull()
                ?.Pipe(s =>
                    Enum.IsDefined(typeof(ButtonStyle), s) ? (ButtonStyle)s : (ButtonStyle?)null
                )
            ?? ButtonStyle.Secondary;

        var label = json.GetPropertyOrNull("label")?.GetStringOrNull();
        var emoji = json.GetPropertyOrNull("emoji")?.Pipe(Emoji.Parse);

        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var customId = json.GetPropertyOrNull("custom_id")?.GetNonWhiteSpaceStringOrNull();
        var skuId = json.GetPropertyOrNull("sku_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        var isDisabled = json.GetPropertyOrNull("disabled")?.GetBooleanOrNull() ?? false;

        return new ButtonComponent(style, label, emoji, url, customId, skuId, isDisabled);
    }
}
