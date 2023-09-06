using System;
using System.Globalization;
using System.Windows.Data;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(Locale), typeof(string))]
public class LocaleToDisplayNameConverter : IValueConverter
{
    public static LocaleToDisplayNameConverter Instance { get; } = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Locale locale ? locale.ToCultureInfo().DisplayName : null;

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
