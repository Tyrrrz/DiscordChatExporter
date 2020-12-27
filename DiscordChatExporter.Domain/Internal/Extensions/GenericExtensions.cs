using System;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class GenericExtensions
    {
        public static T? NullIf<T>(this T value, Func<T, bool> predicate) where T : struct =>
            !predicate(value)
                ? value
                : (T?) null;
    }
}