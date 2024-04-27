using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Gui.Converters;

public class ExportFormatToStringConverter : IValueConverter
{
    public static ExportFormatToStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is ExportFormat format ? format.GetDisplayName() : default;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
