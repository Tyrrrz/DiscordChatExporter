using DiscordChatExporter.Gui.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace DiscordChatExporter.Gui.Converters
{
    [ValueConversion(typeof(DateTimeOffset?), typeof(DateTime?))]
    public class PartitionFormatToTextBoxHintConverter : IValueConverter
    {
        public static PartitionFormatToTextBoxHintConverter Instance { get; } = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PartitionFormat partitionFormat)
                return partitionFormat switch
                {
                    PartitionFormat.Filesize => "MB per partition",
                    PartitionFormat.MessageCount => "Messages per partition",
                    _ => default(string)
                };

            return default(DateTime?);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
