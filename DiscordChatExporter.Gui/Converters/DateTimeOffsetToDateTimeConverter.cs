using System;
using System.Globalization;
using System.Windows.Data;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(DateTimeOffset?), typeof(DateTime?))]
public class DateTimeOffsetToDateTimeConverter : IValueConverter
{
    public static DateTimeOffsetToDateTimeConverter Instance { get; } = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dateTimeOffsetValue)
            return dateTimeOffsetValue.DateTime;

        return default(DateTime?);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTimeValue)
            return new DateTimeOffset(dateTimeValue);

        return default(DateTimeOffset?);
    }
}