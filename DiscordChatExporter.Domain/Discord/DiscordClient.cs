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

        public DiscordClient(AuthToken token, HttpClient httpClient)
        {
            _token = token;
            _httpClient = httpClient;

            // Discord seems to always respond 429 on our first request with unreasonable wait time (10+ minutes).
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
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = _token.GetAuthenticationHeader();

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
                var route = "users/@me/guilds?limit=100";
                if (!string.IsNullOrWhiteSpace(afterId))
                    route += $"&after={afterId}";

                var response = await GetApiResponseAsync(route);

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
            // Special case for direct messages pseudo-guild
            if (guildId == Guild.DirectMessages.Id)
                return Array.Empty<Channel>();

            var response = await GetApiResponseAsync($"guilds/{guildId}/channels");
            var channels = response.EnumerateArray().Select(ParseChannel).ToArray();

            return channels;
        }

        private async Task<Message> GetLastMessageAsync(string channelId, DateTimeOffset? before = null)
        {
            var route = $"channels/{channelId}/messages?limit=1";
            if (before != null)
                route += $"&before={before.Value.ToSnowflake()}";

            var response = await GetApiResponseAsync(route);

            return response.EnumerateArray().Select(ParseMessage).FirstOrDefault();
        }

        public async IAsyncEnumerable<Message> GetMessagesAsync(string channelId,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double>? progress = null)
        {
            // Get the last message
            var lastMessage = await GetLastMessageAsync(channelId, before);

            // If the last message doesn't exist or it's outside of range - return
            if (lastMessage == null || lastMessage.Timestamp < after)
            {
                progress?.Report(1);
                yield break;
            }

            // Get other messages
            var firstMessage = default(Message);
            var afterId = after?.ToSnowflake() ?? "0";
            while (true)
            {
                // Get message batch
                var route = $"channels/{channelId}/messages?limit=100&after={afterId}";
                var response = await GetApiResponseAsync(route);

                // Parse
                var messages = response
                    .EnumerateArray()
                    .Select(ParseMessage)
                    .Reverse() // reverse because messages appear newest first
                    .ToArray();

                // Break if there are no messages (can happen if messages are deleted during execution)
                if (!messages.Any())
                    break;

                // Trim messages to range (until last message)
                var messagesInRange = messages
                    .TakeWhile(m => m.Id != lastMessage.Id && m.Timestamp < lastMessage.Timestamp)
                    .ToArray();

                // Yield messages
                foreach (var message in messagesInRange)
                {
                    // Set first message if it's not set
                    firstMessage ??= message;

                    // Report progress (based on the time range of parsed messages compared to total)
                    progress?.Report((message.Timestamp - firstMessage.Timestamp).TotalSeconds /
                                     (lastMessage.Timestamp - firstMessage.Timestamp).TotalSeconds);

                    yield return message;
                    afterId = message.Id;
                }

                // Break if messages were trimmed (which means the last message was encountered)
                if (messagesInRange.Length != messages.Length)
                    break;
            }

            // Yield last message
            yield return lastMessage;
            progress?.Report(1);
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

            return new HttpClient(handler, true)
            {
                BaseAddress = new Uri("https://discordapp.com/api/v6")
            };
        });
    }
}