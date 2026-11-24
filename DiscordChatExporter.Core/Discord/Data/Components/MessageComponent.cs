using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference
public abstract partial record MessageComponent(MessageComponentKind Kind)
{
    /// <summary>
    /// Child components, for components that can contain other components.
    /// </summary>
    public virtual IReadOnlyList<MessageComponent> Children => [];

    /// <summary>
    /// Enumerates this component and all of its descendants, depth-first.
    /// </summary>
    public IEnumerable<MessageComponent> Flatten()
    {
        yield return this;

        foreach (var child in Children)
        foreach (var descendant in child.Flatten())
            yield return descendant;
    }
}

public abstract partial record MessageComponent
{
    internal static IReadOnlyList<MessageComponent> ParseMany(JsonElement? json) =>
        json?.EnumerateArrayOrNull()?.Select(TryParse).WhereNotNull().ToArray() ?? [];

    public static MessageComponent? TryParse(JsonElement json)
    {
        var kind = (MessageComponentKind?)json.GetPropertyOrNull("type")?.GetInt32OrNull();

        return kind switch
        {
            MessageComponentKind.ActionRow => ActionRowComponent.Parse(json),
            MessageComponentKind.Button => ButtonComponent.Parse(json),
            MessageComponentKind.StringSelect
            or MessageComponentKind.UserSelect
            or MessageComponentKind.RoleSelect
            or MessageComponentKind.MentionableSelect
            or MessageComponentKind.ChannelSelect => SelectMenuComponent.Parse(json, kind.Value),
            MessageComponentKind.Section => SectionComponent.Parse(json),
            MessageComponentKind.TextDisplay => TextDisplayComponent.Parse(json),
            MessageComponentKind.Thumbnail => ThumbnailComponent.Parse(json),
            MessageComponentKind.MediaGallery => MediaGalleryComponent.Parse(json),
            MessageComponentKind.File => FileComponent.Parse(json),
            MessageComponentKind.Separator => SeparatorComponent.Parse(json),
            MessageComponentKind.Container => ContainerComponent.Parse(json),
            // Unknown or unsupported component types are skipped
            _ => null,
        };
    }
}
