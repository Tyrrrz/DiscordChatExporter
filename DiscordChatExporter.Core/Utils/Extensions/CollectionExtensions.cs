using System.Collections.Generic;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<T> ToSingletonEnumerable<T>(this T obj)
    {
        yield return obj;
    }

    public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        var i = 0;
        foreach (var o in source)
            yield return (o, i++);
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        foreach (var o in source)
        {
            if (o is not null)
                yield return o;
        }
    }
}
