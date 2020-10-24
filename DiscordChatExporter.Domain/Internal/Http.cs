using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Polly;

namespace DiscordChatExporter.Domain.Internal
{
    internal static class Http
    {
        public static HttpClient Client { get; } = new HttpClient();

        public static IAsyncPolicy<HttpResponseMessage> ResponsePolicy { get; } =
            Policy
                .Handle<IOException>()
                .Or<HttpRequestException>()
                .OrResult<HttpResponseMessage>(m => m.StatusCode == HttpStatusCode.TooManyRequests)
                .OrResult(m => m.StatusCode == HttpStatusCode.RequestTimeout)
                .OrResult(m => m.StatusCode >= HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(8,
                    (i, result, ctx) =>
                    {
                        // If rate-limited, use retry-after as a guide
                        if (result.Result.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            // Only start respecting retry-after after a few attempts.
                            // The reason is that Discord often sends unreasonable (20+ minutes) retry-after
                            // on the very first request.
                            if (i > 3)
                            {
                                var retryAfterDelay = result.Result.Headers.RetryAfter.Delta;
                                if (retryAfterDelay != null)
                                    return retryAfterDelay.Value + TimeSpan.FromSeconds(1); // margin just in case
                            }
                        }

                        return TimeSpan.FromSeconds(Math.Pow(2, i) + 1);
                    },
                    (response, timespan, retryCount, context) => Task.CompletedTask);

        private static HttpStatusCode? TryGetStatusCodeFromException(HttpRequestException ex)
        {
            // This is extremely frail, but there's no other way
            var statusCodeRaw = Regex.Match(ex.Message, @": (\d+) \(").Groups[1].Value;
            return !string.IsNullOrWhiteSpace(statusCodeRaw)
                ? (HttpStatusCode) int.Parse(statusCodeRaw, CultureInfo.InvariantCulture)
                : (HttpStatusCode?) null;
        }

        public static IAsyncPolicy ExceptionPolicy { get; } =
            Policy
                .Handle<IOException>() // dangerous
                .Or<HttpRequestException>(ex => TryGetStatusCodeFromException(ex) == HttpStatusCode.TooManyRequests)
                .Or<HttpRequestException>(ex => TryGetStatusCodeFromException(ex) == HttpStatusCode.RequestTimeout)
                .Or<HttpRequestException>(ex => TryGetStatusCodeFromException(ex) >= HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(4, i => TimeSpan.FromSeconds(Math.Pow(2, i) + 1));
    }
}