using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiscordChatExporter.Gui.Converters;

public class TimeSpanToDateTimeConverter : IValueConverter
{
    public static TimeSpanToDateTimeConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is TimeSpan timeSpanValue ? DateTime.Today.Add(timeSpanValue) : default(DateTime?);

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is DateTime dateTimeValue ? dateTimeValue.TimeOfDay : default(TimeSpan?);
}
