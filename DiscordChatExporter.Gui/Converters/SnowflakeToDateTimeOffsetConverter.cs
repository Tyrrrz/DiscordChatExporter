using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Gui.Converters;

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
