using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Utils.Extensions;
using Polly;

namespace DiscordChatExporter.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } = new();

    private static bool IsRetryableStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout ||
        // Treat all server-side errors as retryable.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/908
        (int)statusCode >= 500;

    private static bool IsRetryableException(Exception exception) =>
        exception.GetSelfAndChildren().Any(ex =>
            ex is TimeoutException or SocketException or AuthenticationException ||
            ex is HttpRequestException hrex && IsRetryableStatusCode(hrex.TryGetStatusCode() ?? HttpStatusCode.OK)
        );

    public static IAsyncPolicy ResiliencePolicy { get; } =
        Policy
            .Handle<Exception>(IsRetryableException)
            .WaitAndRetryAsync(4, i => TimeSpan.FromSeconds(Math.Pow(2, i) + 1));

    public static IAsyncPolicy<HttpResponseMessage> ResponseResiliencePolicy { get; } =
        Policy
            .Handle<Exception>(IsRetryableException)
            .OrResult<HttpResponseMessage>(m => IsRetryableStatusCode(m.StatusCode))
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
}