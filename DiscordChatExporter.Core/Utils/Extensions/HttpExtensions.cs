using System.Net.Http.Headers;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class HttpExtensions
{
    public static string? TryGetValue(this HttpHeaders headers, string name) =>
        headers.TryGetValues(name, out var values) ? string.Concat(values) : null;
}
