using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class HttpExtensions
{
    public static string? TryGetValue(this HttpHeaders headers, string name) =>
        headers.TryGetValues(name, out var values)
            ? string.Concat(values)
            : null;

    public static HttpStatusCode? TryGetStatusCode(this HttpRequestException ex) =>
        // This is extremely frail, but there's no other way
        Regex
            .Match(ex.Message, @": (\d+) \(")
            .Groups[1]
            .Value
            .NullIfWhiteSpace()?
            .Pipe(s => (HttpStatusCode) int.Parse(s, CultureInfo.InvariantCulture));
}