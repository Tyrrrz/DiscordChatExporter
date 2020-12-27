using System;
using System.Globalization;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class DateExtensions
    {
        public static string ToLocalString(this DateTimeOffset dateTime, string format) =>
            dateTime.ToLocalTime().ToString(format, CultureInfo.InvariantCulture);
    }
}