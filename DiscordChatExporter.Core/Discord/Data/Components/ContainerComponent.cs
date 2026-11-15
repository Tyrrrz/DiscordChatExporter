using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#container
public record ContainerComponent(
    IReadOnlyList<MessageComponent> Components,
    Color? AccentColor,
    bool IsSpoiler
) : MessageComponent(MessageComponentKind.Container)
{
    public override IReadOnlyList<MessageComponent> Children => Components;

    public static ContainerComponent Parse(JsonElement json) =>
        new(
            ParseMany(json.GetPropertyOrNull("components")),
            json.GetPropertyOrNull("accent_color")
                ?.GetInt32OrNull()
                ?.Pipe(Color.FromArgb)
                .WithFullAlpha(),
            json.GetPropertyOrNull("spoiler")?.GetBooleanOrNull() ?? false
        );
}
