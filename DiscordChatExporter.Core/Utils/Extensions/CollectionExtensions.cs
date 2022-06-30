using System.Collections.Generic;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> Enumerate<T>(this T obj)
    {
        yield return obj;
    }

    public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        var i = 0;
        foreach (var o in source)
            yield return (o, i++);
    }
}