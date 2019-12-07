using System;
using System.Drawing;

namespace DiscordChatExporter.Core.Services.Internal
{
    internal static class Extensions
    {
        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime) => new DateTimeOffset(dateTime);

        public static string ToSnowflake(this DateTimeOffset dateTime)
        {
            var value = ((ulong) dateTime.ToUnixTimeMilliseconds() - 1420070400000UL) << 22;
            return value.ToString();
        }

        public static Color ResetAlpha(this Color color) => Color.FromArgb(1, color);
    }
}