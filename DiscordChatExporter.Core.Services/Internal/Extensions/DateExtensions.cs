using System;

namespace DiscordChatExporter.Core.Services.Internal.Extensions
{
    internal static class DateExtensions
    {
        public static string ToSnowflake(this DateTimeOffset dateTime)
        {
            var value = ((ulong) dateTime.ToUnixTimeMilliseconds() - 1420070400000UL) << 22;
            return value.ToString();
        }
    }
}