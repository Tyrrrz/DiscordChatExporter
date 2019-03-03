using System;
using System.Drawing;
using System.Net;

namespace DiscordChatExporter.Core.Internal
{
    internal static class Extensions
    {
        public static string ToSnowflake(this DateTime dateTime)
        {
            const long epoch = 62135596800000;
            var unixTime = dateTime.ToUniversalTime().Ticks / TimeSpan.TicksPerMillisecond - epoch;
            var value = ((ulong) unixTime - 1420070400000UL) << 22;
            return value.ToString();
        }

        public static Color ResetAlpha(this Color color) => Color.FromArgb(1, color);

        public static string HtmlEncode(this string value) => WebUtility.HtmlEncode(value);
    }
}