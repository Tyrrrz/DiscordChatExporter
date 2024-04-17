using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Gui.Converters;

public class SnowflakeToDateTimeStringConverter : IValueConverter
{
    public static SnowflakeToDateTimeStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is Snowflake snowflake ? snowflake.ToDate().ToString("g", culture) : null;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
