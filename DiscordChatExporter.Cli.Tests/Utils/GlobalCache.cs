using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DiscordChatExporter.Cli.Tests.Utils
{
    internal static class GlobalCache
    {
        private static readonly ConcurrentDictionary<string, object?> Dictionary = new();

        public static async Task<T> WrapAsync<T>(string key, Func<Task<T>> getAsync)
        {
            if (Dictionary.TryGetValue(key, out var value) && value is T existing)
                return existing;

            var result = await getAsync();
            Dictionary[key] = result;

            return result;
        }
    }
}