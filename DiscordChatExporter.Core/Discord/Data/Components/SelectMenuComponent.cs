using System.Text.Json;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#string-select
// https://discord.com/developers/docs/components/reference#user-select
// https://discord.com/developers/docs/components/reference#role-select
// https://discord.com/developers/docs/components/reference#mentionable-select
// https://discord.com/developers/docs/components/reference#channel-select
public record SelectMenuComponent(MessageComponentKind Kind, string? Placeholder, bool IsDisabled)
    : MessageComponent(Kind)
{
    public static SelectMenuComponent Parse(JsonElement json, MessageComponentKind kind)
    {
        var placeholder = json.GetPropertyOrNull("placeholder")?.GetNonWhiteSpaceStringOrNull();
        var isDisabled = json.GetPropertyOrNull("disabled")?.GetBooleanOrNull() ?? false;

        return new SelectMenuComponent(kind, placeholder, isDisabled);
    }
}
