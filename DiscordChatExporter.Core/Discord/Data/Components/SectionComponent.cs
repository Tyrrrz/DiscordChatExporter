using System.Collections.Generic;
using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#section
public record SectionComponent(
    IReadOnlyList<MessageComponent> Components,
    MessageComponent? Accessory
) : MessageComponent(MessageComponentKind.Section)
{
    public override IReadOnlyList<MessageComponent> Children =>
        Accessory is not null ? [.. Components, Accessory] : Components;

    public static SectionComponent Parse(JsonElement json) =>
        new(
            ParseMany(json.GetPropertyOrNull("components")),
            json.GetPropertyOrNull("accessory")?.Pipe(TryParse)
        );
}
