using System;
using System.Globalization;
using System.Windows.Data;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(ExportFormat), typeof(string))]
public class ExportFormatToStringConverter : IValueConverter
{
    public static ExportFormatToStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is ExportFormat exportFormatValue ? exportFormatValue.GetDisplayName() : default;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
