using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DiscordChatExporter.Gui.Converters;

public class LocaleToDisplayNameConverter : IValueConverter
{
    public static LocaleToDisplayNameConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string locale && !string.IsNullOrWhiteSpace(locale)
            ? CultureInfo.GetCultureInfo(locale).DisplayName
            : "System default";

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
