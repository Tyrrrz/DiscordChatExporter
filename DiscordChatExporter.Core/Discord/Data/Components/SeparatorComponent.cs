using System.Text.Json;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#separator
public record SeparatorComponent(bool IsDivider, bool IsLarge)
    : MessageComponent(MessageComponentKind.Separator)
{
    public static SeparatorComponent Parse(JsonElement json) =>
        new(
            json.GetPropertyOrNull("divider")?.GetBooleanOrNull() ?? true,
            json.GetPropertyOrNull("spacing")?.GetInt32OrNull() == 2
        );
}
