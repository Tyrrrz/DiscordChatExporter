using System;
using System.Net;
using Tyrrrz.Extensions;

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

        public static string Base64Encode(this string str)
        {
            return str.GetBytes().ToBase64();
        }

        public static string Base64Decode(this string str)
        {
            return str.FromBase64().GetString();
        }

        public static string HtmlEncode(this string str)
        {
            return WebUtility.HtmlEncode(str);
        }

        public static string HtmlDecode(this string str)
        {
            return WebUtility.HtmlDecode(str);
        }
    }
}