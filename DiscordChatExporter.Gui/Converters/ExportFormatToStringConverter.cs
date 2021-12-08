using System;
using System.Globalization;
using System.Windows.Data;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(ExportFormat), typeof(string))]
public class ExportFormatToStringConverter : IValueConverter
{
    public static ExportFormatToStringConverter Instance { get; } = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ExportFormat exportFormatValue)
            return exportFormatValue.GetDisplayName();

        return default(string?);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}