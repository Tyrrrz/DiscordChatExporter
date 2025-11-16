using System.Net.Http.Headers;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class HttpExtensions
{
    extension(HttpHeaders headers)
    {
        public string? TryGetValue(string name) =>
            headers.TryGetValues(name, out var values) ? string.Concat(values) : null;
    }
}
