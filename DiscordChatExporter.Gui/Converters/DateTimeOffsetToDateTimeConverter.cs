using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiscordChatExporter.Gui.Converters;

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
