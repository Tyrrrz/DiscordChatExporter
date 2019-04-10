using System;
using System.Globalization;
using System.Windows.Data;

namespace DiscordChatExporter.Gui.Converters
{
    [ValueConversion(typeof(DateTimeOffset), typeof(DateTime))]
    public class DateTimeOffsetToDateTimeConverter : IValueConverter
    {
        public static DateTimeOffsetToDateTimeConverter Instance { get; } = new DateTimeOffsetToDateTimeConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset date)
                return date.DateTime;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return new DateTimeOffset(date);

            return null;
        }
    }
}