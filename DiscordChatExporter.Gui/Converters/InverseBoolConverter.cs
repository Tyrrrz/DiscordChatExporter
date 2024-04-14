using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiscordChatExporter.Gui.Converters;

public class InverseBoolConverter : IValueConverter
{
    public static InverseBoolConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is false;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is false;
}
