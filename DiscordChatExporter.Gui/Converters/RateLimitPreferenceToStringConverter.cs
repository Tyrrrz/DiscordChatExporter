using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Gui.Converters;

public class RateLimitPreferenceToStringConverter : IValueConverter
{
    public static RateLimitPreferenceToStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is RateLimitPreference rateLimitPreference
            ? rateLimitPreference.GetDisplayName()
            : default;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
