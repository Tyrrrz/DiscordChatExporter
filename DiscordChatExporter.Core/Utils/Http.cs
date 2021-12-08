using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Utils.Extensions;
using Polly;

namespace DiscordChatExporter.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } = new();

    public static IAsyncPolicy<HttpResponseMessage> ResponsePolicy { get; } =
        Policy
            .Handle<IOException>()
            .Or<HttpRequestException>()
            .OrResult<HttpResponseMessage>(m => m.StatusCode == HttpStatusCode.TooManyRequests)
            .OrResult(m => m.StatusCode == HttpStatusCode.RequestTimeout)
            .OrResult(m => m.StatusCode >= HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(
                8,
                (i, result, _) =>
                {
                    // If rate-limited, use retry-after as a guide
                    if (result.Result?.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        // Only start respecting retry-after after a few attempts, because
                        // Discord often sends unreasonable (20+ minutes) retry-after
                        // on the very first request.
                        if (i > 3)
                        {
                            var retryAfterDelay = result.Result.Headers.RetryAfter?.Delta;
                            if (retryAfterDelay is not null)
                                return retryAfterDelay.Value + TimeSpan.FromSeconds(1); // margin just in case
                        }
                    }

                    return TimeSpan.FromSeconds(Math.Pow(2, i) + 1);
                },
                (_, _, _, _) => Task.CompletedTask
            );

    private static HttpStatusCode? TryGetStatusCodeFromException(HttpRequestException ex) =>
        // This is extremely frail, but there's no other way
        Regex
            .Match(ex.Message, @": (\d+) \(")
            .Groups[1]
            .Value
            .NullIfWhiteSpace()?
            .Pipe(s => (HttpStatusCode) int.Parse(s, CultureInfo.InvariantCulture));

    public static IAsyncPolicy ExceptionPolicy { get; } =
        Policy
            .Handle<IOException>() // dangerous
            .Or<HttpRequestException>(ex =>
                TryGetStatusCodeFromException(ex) is
                    HttpStatusCode.TooManyRequests or
                    HttpStatusCode.RequestTimeout or
                    HttpStatusCode.InternalServerError
            )
            .WaitAndRetryAsync(4, i => TimeSpan.FromSeconds(Math.Pow(2, i) + 1));
}