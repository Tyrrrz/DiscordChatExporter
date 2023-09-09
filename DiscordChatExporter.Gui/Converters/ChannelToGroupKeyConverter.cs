using System;
using System.Globalization;
using System.Windows.Data;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(Channel), typeof(string))]
public class ChannelToGroupKeyConverter : IValueConverter
{
    public static ChannelToGroupKeyConverter Instance { get; } = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            Channel { IsThread: true, Parent: not null } channel
                => $"Threads in #{channel.Parent.Name}",

            Channel channel => channel.Parent?.Name ?? "???",

            _ => null
        };

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
