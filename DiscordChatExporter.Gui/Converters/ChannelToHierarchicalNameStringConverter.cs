using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Gui.Converters;

public class ChannelToHierarchicalNameStringConverter : IValueConverter
{
    public static ChannelToHierarchicalNameStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is Channel channel ? channel.GetHierarchicalName() : null;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
