﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace DiscordChatExporter.Gui.Converters;

[ValueConversion(typeof(string), typeof(string))]
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
