using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://docs.discord.com/developers/components/reference#component-object
public partial record MessageComponent(
    MessageComponentType Kind,
    IReadOnlyList<MessageComponent> Components,
    ButtonComponent? Button
)
{
    public bool HasButtons => Button is not null || Components.Any(c => c.HasButtons);

    public IReadOnlyList<ButtonComponent> Buttons =>
        Components.Select(c => c.Button).WhereNotNull().ToArray();
}

public partial record MessageComponent
{
    public static MessageComponent? Parse(JsonElement json)
    {
        var rawType = json.GetPropertyOrNull("type")?.GetInt32OrNull();
        if (rawType is null)
            return null;

        var type = rawType.Value;
        if (!Enum.IsDefined(typeof(MessageComponentType), type))
            return null;

        return Parse((MessageComponentType)type, json);
    }

    private static MessageComponent Parse(MessageComponentType type, JsonElement json)
    {
        return type switch
        {
            MessageComponentType.Button => ParseButton(json),
            _ => ParseDefault(type, json),
        };
    }

    private static MessageComponent ParseDefault(MessageComponentType type, JsonElement json)
    {
        var components = ParseComponents(json);

        return new MessageComponent(type, components, null);
    }

    private static MessageComponent ParseButton(JsonElement json)
    {
        var components = ParseComponents(json);
        var button = ButtonComponent.Parse(json);

        return new MessageComponent(MessageComponentType.Button, components, button);
    }

    private static MessageComponent[] ParseComponents(JsonElement json)
    {
        return json.GetPropertyOrNull("components")
                ?.EnumerateArrayOrNull()
                ?.Select(Parse)
                .WhereNotNull()
                .ToArray()
            ?? [];
    }
}
