using System;
using System.Globalization;
using System.Windows.Data;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Gui.Converters
{
    [ValueConversion(typeof(ExportFormat), typeof(string))]
    public class ExportFormatToStringConverter : IValueConverter
    {
        public static ExportFormatToStringConverter Instance { get; } = new ExportFormatToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var format = (ExportFormat?) value;
            return format?.GetDisplayName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}