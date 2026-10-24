using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Utils.Logging;
using Polly;
using Polly.Retry;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } = new();

    private static bool IsRetryableStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout
        ||
        // Treat all server-side errors as retryable
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/908
        (int)statusCode >= 500;

    private static bool IsRetryableException(Exception exception) =>
        exception
            .GetSelfAndDescendants()
            .Any(ex =>
                ex is TimeoutException or SocketException or AuthenticationException
                || ex is HttpRequestException hrex
                    && IsRetryableStatusCode(hrex.StatusCode ?? HttpStatusCode.OK)
            );

    public static ResiliencePipeline ResiliencePipeline { get; } =
        new ResiliencePipelineBuilder()
            .AddRetry(
                new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(IsRetryableException),
                    MaxRetryAttempts = 4,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(1),
                    OnRetry = args =>
                    {
                        var reason = args.Outcome.Exception is { } ex
                            ? $"{ex.GetType().Name}: {ex.Message}"
                            : "unknown error";

                        Logger.Warning(
                            LogCategory.Http,
                            $"HTTP request failed ({reason}). Retrying (attempt {args.AttemptNumber + 1}/4) in {args.RetryDelay.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)}s..."
                        );

                        return default;
                    },
                }
            )
            .Build();

    public static ResiliencePipeline<HttpResponseMessage> ResponseResiliencePipeline { get; } =
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(
                new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<Exception>(IsRetryableException)
                        .HandleResult(m => IsRetryableStatusCode(m.StatusCode)),
                    MaxRetryAttempts = 8,
                    DelayGenerator = args =>
                    {
                        // If rate-limited, use retry-after header as the guide.
                        // The response can be null here if an exception was thrown.
                        if (args.Outcome.Result?.Headers.RetryAfter?.Delta is { } retryAfter)
                        {
                            // Add some buffer just in case
                            return ValueTask.FromResult<TimeSpan?>(
                                retryAfter + TimeSpan.FromSeconds(1)
                            );
                        }

                        return ValueTask.FromResult<TimeSpan?>(
                            TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber) + 1)
                        );
                    },
                    OnRetry = args =>
                    {
                        var response = args.Outcome.Result;
                        var requestUri = response?.RequestMessage?.RequestUri?.PathAndQuery;
                        var target = !string.IsNullOrWhiteSpace(requestUri)
                            ? $" '{requestUri}'"
                            : "";

                        var reason =
                            response is not null
                                ? $"HTTP {(int)response.StatusCode} {response.StatusCode}"
                            : args.Outcome.Exception is { } ex
                                ? $"{ex.GetType().Name}: {ex.Message}"
                            : "unknown error";

                        var retryAfterInfo = response?.Headers.RetryAfter?.Delta is { } retryAfter
                            ? $" (Retry-After: {retryAfter.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)}s)"
                            : "";

                        Logger.Warning(
                            LogCategory.Http,
                            $"API request{target} failed with {reason}{retryAfterInfo}. Retrying (attempt {args.AttemptNumber + 1}/8) in {args.RetryDelay.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)}s..."
                        );

                        return default;
                    },
                }
            )
            .Build();
}
