using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#component-object-component-types
public enum MessageComponentKind
{
    ActionRow = 1,
    Button = 2,
    StringSelect = 3,
    TextInput = 4,
    UserSelect = 5,
    RoleSelect = 6,
    MentionableSelect = 7,
    ChannelSelect = 8,
    Section = 9,
    TextDisplay = 10,
    Thumbnail = 11,
    MediaGallery = 12,
    File = 13,
    Separator = 14,
    Container = 17,
}

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
        var kind = json.GetPropertyOrNull("type")?.GetInt32OrNull();

        return (MessageComponentKind?)kind switch
        {
            MessageComponentKind.ActionRow => ActionRowComponent.Parse(json),
            MessageComponentKind.Button => ButtonComponent.Parse(json),
            MessageComponentKind.StringSelect
            or MessageComponentKind.UserSelect
            or MessageComponentKind.RoleSelect
            or MessageComponentKind.MentionableSelect
            or MessageComponentKind.ChannelSelect => SelectMenuComponent.Parse(json),
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

// https://discord.com/developers/docs/components/reference#unfurled-media-item
public record UnfurledMediaItem(
    string Url,
    string? ProxyUrl,
    int? Width,
    int? Height,
    string? ContentType,
    Snowflake? AttachmentId
)
{
    public bool IsVideo => ContentType?.StartsWith("video/") == true;

    /// <summary>
    /// Whether this media item points at the specified message attachment.
    /// </summary>
    public bool Refers(Attachment attachment) =>
        AttachmentId == attachment.Id
        || string.Equals(Url, attachment.Url, StringComparison.Ordinal)
        || string.Equals(
            Url,
            $"attachment://{attachment.FileName}",
            StringComparison.OrdinalIgnoreCase
        );

    public static UnfurledMediaItem? TryParse(JsonElement json)
    {
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        if (url is null)
            return null;

        var proxyUrl = json.GetPropertyOrNull("proxy_url")?.GetNonWhiteSpaceStringOrNull();
        var width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
        var height = json.GetPropertyOrNull("height")?.GetInt32OrNull();
        var contentType = json.GetPropertyOrNull("content_type")?.GetNonWhiteSpaceStringOrNull();
        var attachmentId = json.GetPropertyOrNull("attachment_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(s => Snowflake.TryParse(s));

        return new UnfurledMediaItem(url, proxyUrl, width, height, contentType, attachmentId);
    }
}

// https://discord.com/developers/docs/components/reference#action-row
public record ActionRowComponent(IReadOnlyList<MessageComponent> Components)
    : MessageComponent(MessageComponentKind.ActionRow)
{
    public override IReadOnlyList<MessageComponent> Children => Components;

    public static ActionRowComponent Parse(JsonElement json) =>
        new(ParseMany(json.GetPropertyOrNull("components")));
}

// https://discord.com/developers/docs/components/reference#button-button-styles
public enum ButtonStyle
{
    Primary = 1,
    Secondary = 2,
    Success = 3,
    Danger = 4,
    Link = 5,
    Premium = 6,
}

// https://discord.com/developers/docs/components/reference#button
public record ButtonComponent(
    ButtonStyle Style,
    string? Label,
    Emoji? Emoji,
    string? Url,
    bool IsDisabled
) : MessageComponent(MessageComponentKind.Button)
{
    public static ButtonComponent Parse(JsonElement json)
    {
        var style =
            json.GetPropertyOrNull("style")?.GetInt32OrNull()?.Pipe(s => (ButtonStyle)s)
            ?? ButtonStyle.Secondary;

        var label = json.GetPropertyOrNull("label")?.GetNonWhiteSpaceStringOrNull();
        var emoji = json.GetPropertyOrNull("emoji")?.Pipe(Emoji.Parse);
        var url = json.GetPropertyOrNull("url")?.GetNonWhiteSpaceStringOrNull();
        var isDisabled = json.GetPropertyOrNull("disabled")?.GetBooleanOrNull() ?? false;

        return new ButtonComponent(style, label, emoji, url, isDisabled);
    }
}

// https://discord.com/developers/docs/components/reference#string-select
public record SelectMenuComponent(string? Placeholder, bool IsDisabled)
    : MessageComponent(MessageComponentKind.StringSelect)
{
    public static SelectMenuComponent Parse(JsonElement json)
    {
        var placeholder = json.GetPropertyOrNull("placeholder")?.GetNonWhiteSpaceStringOrNull();
        var isDisabled = json.GetPropertyOrNull("disabled")?.GetBooleanOrNull() ?? false;

        return new SelectMenuComponent(placeholder, isDisabled);
    }
}

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

// https://discord.com/developers/docs/components/reference#text-display
public record TextDisplayComponent(string Content)
    : MessageComponent(MessageComponentKind.TextDisplay)
{
    public static TextDisplayComponent Parse(JsonElement json) =>
        new(json.GetPropertyOrNull("content")?.GetStringOrNull() ?? "");
}

// https://discord.com/developers/docs/components/reference#thumbnail
public record ThumbnailComponent(UnfurledMediaItem? Media, string? Description, bool IsSpoiler)
    : MessageComponent(MessageComponentKind.Thumbnail)
{
    public static ThumbnailComponent Parse(JsonElement json) =>
        new(
            json.GetPropertyOrNull("media")?.Pipe(UnfurledMediaItem.TryParse),
            json.GetPropertyOrNull("description")?.GetNonWhiteSpaceStringOrNull(),
            json.GetPropertyOrNull("spoiler")?.GetBooleanOrNull() ?? false
        );
}

// https://discord.com/developers/docs/components/reference#media-gallery-media-gallery-item-structure
public record MediaGalleryItem(UnfurledMediaItem Media, string? Description, bool IsSpoiler)
{
    public static MediaGalleryItem? TryParse(JsonElement json)
    {
        var media = json.GetPropertyOrNull("media")?.Pipe(UnfurledMediaItem.TryParse);
        if (media is null)
            return null;

        return new MediaGalleryItem(
            media,
            json.GetPropertyOrNull("description")?.GetNonWhiteSpaceStringOrNull(),
            json.GetPropertyOrNull("spoiler")?.GetBooleanOrNull() ?? false
        );
    }
}

// https://discord.com/developers/docs/components/reference#media-gallery
public record MediaGalleryComponent(IReadOnlyList<MediaGalleryItem> Items)
    : MessageComponent(MessageComponentKind.MediaGallery)
{
    public static MediaGalleryComponent Parse(JsonElement json) =>
        new(
            json.GetPropertyOrNull("items")
                ?.EnumerateArrayOrNull()
                ?.Select(MediaGalleryItem.TryParse)
                .WhereNotNull()
                .ToArray()
                ?? []
        );
}

// https://discord.com/developers/docs/components/reference#file
public record FileComponent(UnfurledMediaItem? File, string? Name, FileSize? Size, bool IsSpoiler)
    : MessageComponent(MessageComponentKind.File)
{
    public static FileComponent Parse(JsonElement json) =>
        new(
            json.GetPropertyOrNull("file")?.Pipe(UnfurledMediaItem.TryParse),
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull(),
            json.GetPropertyOrNull("size")?.GetInt64OrNull()?.Pipe(FileSize.FromBytes),
            json.GetPropertyOrNull("spoiler")?.GetBooleanOrNull() ?? false
        );
}

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
