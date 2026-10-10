using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Gui.Converters;

public class ForumAssetOptionToStringConverter : IValueConverter
{
    public static ForumAssetOptionToStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value switch
        {
            ForumCommonAssetMode.SharedFolder => "One shared folder for the whole export",
            ForumCommonAssetMode.PerThreadFolder => "Separate common folder inside every post",
            ForumCommonAssetMode.Skip => "Do not download common resources",

            ForumAttachmentFolderMode.PerThread => "One attachments folder per post",
            ForumAttachmentFolderMode.ByMediaType => "Split into images, videos, audio and files",
            ForumAttachmentFolderMode.ByMessage => "Separate folder for every message",

            ForumAttachmentNamingMode.OriginalWithHash => "Original name + safety hash",
            ForumAttachmentNamingMode.AttachmentIdAndOriginal => "Attachment ID + original name",
            ForumAttachmentNamingMode.MessageAndAttachmentIdsAndOriginal =>
                "Message ID + attachment ID + original name",
            _ => value?.ToString(),
        };

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
