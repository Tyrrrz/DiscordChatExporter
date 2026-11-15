using System.Collections.Generic;
using System.Text.Json;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#action-row
public record ActionRowComponent(IReadOnlyList<MessageComponent> Components)
    : MessageComponent(MessageComponentKind.ActionRow)
{
    public override IReadOnlyList<MessageComponent> Children => Components;

    public static ActionRowComponent Parse(JsonElement json) =>
        new(ParseMany(json.GetPropertyOrNull("components")));
}
