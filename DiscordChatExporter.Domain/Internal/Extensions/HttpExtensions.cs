using System.Net.Http.Headers;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class HttpExtensions
    {
        public static string? TryGetValue(this HttpContentHeaders headers, string name) =>
            headers.TryGetValues(name, out var values)
                ? string.Concat(values)
                : null;
    }
}