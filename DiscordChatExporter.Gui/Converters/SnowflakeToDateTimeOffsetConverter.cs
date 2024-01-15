using System;
using System.Globalization;
using System.Windows.Data;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(Snowflake?), typeof(DateTimeOffset?))]
public class SnowflakeToDateTimeOffsetConverter : IValueConverter
{
    public static SnowflakeToDateTimeOffsetConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is Snowflake snowflake ? snowflake.ToDate() : null;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
