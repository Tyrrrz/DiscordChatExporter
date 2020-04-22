using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordChatExporter.Domain.Utilities
{
    public static class AsyncExtensions
    {
        private static async ValueTask<IReadOnlyList<T>> AggregateAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();

            await foreach (var i in asyncEnumerable)
                list.Add(i);

            return list;
        }

        public static ValueTaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>(this IAsyncEnumerable<T> asyncEnumerable) =>
            asyncEnumerable.AggregateAsync().GetAwaiter();

        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> handleAsync, int degreeOfParallelism)
        {
            using var semaphore = new SemaphoreSlim(degreeOfParallelism);

            await Task.WhenAll(source.Select(async item =>
            {
                // ReSharper disable once AccessToDisposedClosure
                await semaphore.WaitAsync();

                try
                {
                    await handleAsync(item);
                }
                finally
                {
                    // ReSharper disable once AccessToDisposedClosure
                    semaphore.Release();
                }
            }));
        }
    }
}