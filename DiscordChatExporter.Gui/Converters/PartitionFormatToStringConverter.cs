using DiscordChatExporter.Domain.Exporting;
using DiscordChatExporter.Gui.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace DiscordChatExporter.Gui.Converters
{
    [ValueConversion(typeof(ExportFormat), typeof(string))]
    public class PartitionFormatToStringConverter : IValueConverter
    {
        public static PartitionFormatToStringConverter Instance { get; } = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PartitionFormat partitionFormatValue)
                return partitionFormatValue.GetDisplayName();

            return default(string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
