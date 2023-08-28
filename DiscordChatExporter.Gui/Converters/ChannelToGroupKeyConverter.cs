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
            Channel channel when channel.Kind.IsThread()
                => $"Threads in #{channel.ParentNameWithFallback}",

            Channel channel => channel.ParentNameWithFallback,

            _ => null
        };

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
