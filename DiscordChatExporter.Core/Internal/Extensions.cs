using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public static IEnumerable<IReadOnlyList<T>> GroupAdjacentWhile<T>(this IEnumerable<T> source,
            Func<IReadOnlyList<T>, T, bool> groupPredicate)
        {
            // Create buffer
            var buffer = new List<T>();

            // Enumerate source
            foreach (var element in source)
            {
                // If buffer is not empty and group predicate failed - yield and flush buffer
                if (buffer.Any() && !groupPredicate(buffer, element))
                {
                    yield return buffer;
                    buffer = new List<T>(); // new instance to reset reference
                }

                // Add element to buffer
                buffer.Add(element);
            }

            // If buffer still has something after the source has been enumerated - yield
            if (buffer.Any())
                yield return buffer;
        }

        public static IEnumerable<IReadOnlyList<T>> GroupAdjacentWhile<T>(this IEnumerable<T> source,
            Func<IReadOnlyList<T>, bool> groupPredicate)
            => source.GroupAdjacentWhile((buffer, _) => groupPredicate(buffer));
    }
}