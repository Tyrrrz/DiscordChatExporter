using System;
using System.Globalization;
using System.Windows.Data;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(DateTimeOffset?), typeof(DateTime?))]
public class DateTimeOffsetToDateTimeConverter : IValueConverter
{
    public static DateTimeOffsetToDateTimeConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is DateTimeOffset dateTimeOffsetValue
            ? dateTimeOffsetValue.DateTime
            : default(DateTime?);

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is DateTime dateTimeValue
            ? new DateTimeOffset(dateTimeValue)
            : default(DateTimeOffset?);
}
