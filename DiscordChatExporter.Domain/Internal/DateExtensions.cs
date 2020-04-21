using System;
using System.Globalization;

namespace DiscordChatExporter.Domain.Internal
{
    internal static class DateExtensions
    {
        public static string ToSnowflake(this DateTimeOffset dateTime)
        {
            var value = ((ulong) dateTime.ToUnixTimeMilliseconds() - 1420070400000UL) << 22;
            return value.ToString();
        }

        public static string ToLocalString(this DateTimeOffset dateTime, string format) =>
            dateTime.ToLocalTime().ToString(format, CultureInfo.InvariantCulture);
    }
}