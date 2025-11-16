using System.Collections.Generic;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class CollectionExtensions
{
    extension<T>(T obj)
    {
        public IEnumerable<T> ToSingletonEnumerable()
        {
            yield return obj;
        }
    }

    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<(T value, int index)> WithIndex()
        {
            var i = 0;
            foreach (var o in source)
                yield return (o, i++);
        }
    }

    extension<T>(IEnumerable<T?> source)
        where T : class
    {
        public IEnumerable<T> WhereNotNull()
        {
            foreach (var o in source)
            {
                if (o is not null)
                    yield return o;
            }
        }
    }
}
