using System;
using System.Globalization;
using System.Windows.Data;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(TimeSpan?), typeof(DateTime?))]
public class TimeSpanToDateTimeConverter : IValueConverter
{
    public static TimeSpanToDateTimeConverter Instance { get; } = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpanValue)
            return DateTime.Today.Add(timeSpanValue);

        return default(DateTime?);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTimeValue)
            return dateTimeValue.TimeOfDay;

        return default(TimeSpan?);
    }
}