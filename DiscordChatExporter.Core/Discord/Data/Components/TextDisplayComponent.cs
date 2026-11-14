using System.Text.Json;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#text-display
public record TextDisplayComponent(string Content)
    : MessageComponent(MessageComponentKind.TextDisplay)
{
    public static TextDisplayComponent Parse(JsonElement json) =>
        new(json.GetPropertyOrNull("content")?.GetStringOrNull() ?? "");
}
