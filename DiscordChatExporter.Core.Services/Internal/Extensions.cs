using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

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

        public static async Task<IReadOnlyList<T>> AggregateAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();
            
            await foreach (var i in asyncEnumerable)
                list.Add(i);

            return list;
        }
    }
}