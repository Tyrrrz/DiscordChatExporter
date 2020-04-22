using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exceptions;
using DiscordChatExporter.Domain.Internal;
using Polly;

namespace DiscordChatExporter.Domain.Discord
{
    public partial class DiscordClient
    {
        private readonly AuthToken _token;
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _httpRequestPolicy;

        private readonly Uri _baseUri = new Uri("https://discordapp.com/api/v6/", UriKind.Absolute);

        public DiscordClient(AuthToken token, HttpClient httpClient)
        {
            _token = token;
            _httpClient = httpClient;

            // Discord seems to always respond with 429 on the first request with unreasonable wait time (10+ minutes).
            // For that reason the policy will start respecting their retry-after header only after Nth failed response.
            _httpRequestPolicy = Policy
                .HandleResult<HttpResponseMessage>(m => m.StatusCode == HttpStatusCode.TooManyRequests)
                .OrResult(m => m.StatusCode >= HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(6,
                    (i, result, ctx) =>
                    {
                        if (i <= 3)
                            return TimeSpan.FromSeconds(2 * i);

                        if (i <= 5)
                            return TimeSpan.FromSeconds(5 * i);

                        return result.Result.Headers.RetryAfter.Delta ?? TimeSpan.FromSeconds(10 * i);
                    },
                    (response, timespan, retryCount, context) => Task.CompletedTask);
        }

        public DiscordClient(AuthToken token)
            : this(token, LazyHttpClient.Value)
        {
        }

        private async Task<JsonElement> GetApiResponseAsync(string url)
        {
            using var response = await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, url));
                request.Headers.Authorization = _token.GetAuthorizationHeader();

                return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw DiscordChatExporterException.Unauthorized();

            if ((int) response.StatusCode >= 400)
                throw DiscordChatExporterException.FailedHttpRequest(response);

            return await response.Content.ReadAsJsonAsync();
        }

        // TODO: do we need this?
        private async Task<JsonElement?> TryGetApiResponseAsync(string url)
        {
            try
            {
                return await GetApiResponseAsync(url);
            }
            catch (DiscordChatExporterException)
            {
                return null;
            }
        }

        public async Task<Guild> GetGuildAsync(string guildId)
        {
            // Special case for direct messages pseudo-guild
            if (guildId == Guild.DirectMessages.Id)
                return Guild.DirectMessages;

            var response = await GetApiResponseAsync($"guilds/{guildId}");
            var guild = ParseGuild(response);

            return guild;
        }

        public async Task<Member?> GetGuildMemberAsync(string guildId, string userId)
        {
            var response = await TryGetApiResponseAsync($"guilds/{guildId}/members/{userId}");
            return response?.Pipe(ParseMember);
        }

        public async Task<Channel> GetChannelAsync(string channelId)
        {
            var response = await GetApiResponseAsync($"channels/{channelId}");
            var channel = ParseChannel(response);

            return channel;
        }

        public async IAsyncEnumerable<Guild> GetUserGuildsAsync()
        {
            var afterId = "";

            while (true)
            {
                var url = new UrlBuilder()
                    .SetPath("users/@me/guilds")
                    .SetQueryParameter("limit", "100")
                    .SetQueryParameterIfNotNullOrWhiteSpace("after", afterId)
                    .Build();

                var response = await GetApiResponseAsync(url);

                var isEmpty = true;

                // Get full guild object
                foreach (var guildJson in response.EnumerateArray())
                {
                    var guildId = ParseId(guildJson);

                    yield return await GetGuildAsync(guildId);
                    afterId = guildId;

                    isEmpty = false;
                }

                if (isEmpty)
                    yield break;
            }
        }

        public async Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync()
        {
            var response = await GetApiResponseAsync("users/@me/channels");
            var channels = response.EnumerateArray().Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(string guildId)
        {
            // Direct messages pseudo-guild
            if (guildId == Guild.DirectMessages.Id)
                return Array.Empty<Channel>();

            var response = await GetApiResponseAsync($"guilds/{guildId}/channels");
            var channels = response.EnumerateArray().Select(ParseChannel).ToArray();

            return channels;
        }

        private async Task<Message> GetLastMessageAsync(string channelId, DateTimeOffset? before = null)
        {
            var url = new UrlBuilder()
                .SetPath($"channels/{channelId}/messages")
                .SetQueryParameter("limit", "1")
                .SetQueryParameterIfNotNullOrWhiteSpace("before", before?.ToSnowflake())
                .Build();

            var response = await GetApiResponseAsync(url);

            return response.EnumerateArray().Select(ParseMessage).FirstOrDefault();
        }

        public async IAsyncEnumerable<Message> GetMessagesAsync(
            string channelId,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null,
            IProgress<double>? progress = null)
        {
            var lastMessage = await GetLastMessageAsync(channelId, before);

            // If the last message doesn't exist or it's outside of range - return
            if (lastMessage == null || lastMessage.Timestamp < after)
                yield break;

            var firstMessage = default(Message);
            var afterId = after?.ToSnowflake() ?? "0";

            while (true)
            {
                var url = new UrlBuilder()
                    .SetPath($"channels/{channelId}/messages")
                    .SetQueryParameter("limit", "100")
                    .SetQueryParameter("after", afterId)
                    .Build();

                var response = await GetApiResponseAsync(url);

                var messages = response
                    .EnumerateArray()
                    .Select(ParseMessage)
                    .Reverse() // reverse because messages appear newest first
                    .ToArray();

                // Break if there are no messages (can happen if messages are deleted during execution)
                if (!messages.Any())
                    break;

                foreach (var message in messages)
                {
                    firstMessage ??= message;

                    // Ensure messages are in range (take into account that last message could have been deleted)
                    if (message.Timestamp > lastMessage.Timestamp)
                        yield break;

                    // Report progress based on the duration of parsed messages divided by total
                    progress?.Report(
                        (message.Timestamp - firstMessage.Timestamp) /
                        (lastMessage.Timestamp - firstMessage.Timestamp)
                    );

                    yield return message;
                    afterId = message.Id;

                    // Yielded last message - break loop
                    if (message.Id == lastMessage.Id)
                        yield break;
                }
            }
        }
    }

    public partial class DiscordClient
    {
        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler();

            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            handler.UseCookies = false;

            return new HttpClient(handler, true);
        });
    }
}