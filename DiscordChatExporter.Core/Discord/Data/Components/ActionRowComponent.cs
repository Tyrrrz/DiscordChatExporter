using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#action-row
public partial record ActionRowComponent(IReadOnlyList<ButtonComponent> Components)
{
    public bool HasButtons => Components.Any();
}

public partial record ActionRowComponent
{
    public static ActionRowComponent? Parse(JsonElement json)
    {
        var type = json.GetPropertyOrNull("type")?.GetInt32OrNull();
        if (type != 1)
            return null;

        var components =
            json.GetPropertyOrNull("components")
                ?.EnumerateArrayOrNull()
                ?.Where(c => c.GetPropertyOrNull("type")?.GetInt32OrNull() == 2)
                ?.Select(ButtonComponent.Parse)
                .ToArray()
            ?? [];

        return new ActionRowComponent(components);
    }
}
