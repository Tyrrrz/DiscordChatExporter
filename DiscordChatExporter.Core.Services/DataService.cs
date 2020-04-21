using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services.Exceptions;
using DiscordChatExporter.Core.Services.Internal;
using DiscordChatExporter.Core.Services.Internal.Extensions;
using Polly;

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService : IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IAsyncPolicy<HttpResponseMessage> _httpPolicy;

        public DataService()
        {
            _httpClient.BaseAddress = new Uri("https://discordapp.com/api/v6");

            // Discord seems to always respond 429 on our first request with unreasonable wait time (10+ minutes).
            // For that reason the policy will start respecting their retry-after header only after Nth failed response.
            _httpPolicy = Policy
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

        private async Task<JsonElement> GetApiResponseAsync(AuthToken token, string route)
        {
            return (await GetApiResponseAsync(token, route, true))!.Value;
        }

        private async Task<JsonElement?> GetApiResponseAsync(AuthToken token, string route, bool errorOnFail)
        {
            using var response = await _httpPolicy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, route);

                request.Headers.Authorization = token.Type == AuthTokenType.Bot
                    ? new AuthenticationHeaderValue("Bot", token.Value)
                    : new AuthenticationHeaderValue(token.Value);

                return await _httpClient.SendAsync(request);
            });

            // We throw our own exception here because default one doesn't have status code
            if (!response.IsSuccessStatusCode)
            {
                if (errorOnFail)
                    throw new HttpErrorStatusCodeException(response.StatusCode, response.ReasonPhrase);

                return null;
            }

            var jsonRaw = await response.Content.ReadAsStringAsync();
            return Json.Parse(jsonRaw);
        }

        public async Task<Guild> GetGuildAsync(AuthToken token, string guildId)
        {
            // Special case for direct messages pseudo-guild
            if (guildId == Guild.DirectMessages.Id)
                return Guild.DirectMessages;

            var response = await GetApiResponseAsync(token, $"guilds/{guildId}");
            var guild = ParseGuild(response);

            return guild;
        }

        public async Task<Member?> GetGuildMemberAsync(AuthToken token, string guildId, string userId)
        {
            var response = await GetApiResponseAsync(token, $"guilds/{guildId}/members/{userId}", false);
            return response?.Pipe(ParseMember);
        }

        public async Task<Channel> GetChannelAsync(AuthToken token, string channelId)
        {
            var response = await GetApiResponseAsync(token, $"channels/{channelId}");
            var channel = ParseChannel(response);

            return channel;
        }

        public async IAsyncEnumerable<Guild> GetUserGuildsAsync(AuthToken token)
        {
            var afterId = "";

            while (true)
            {
                var route = "users/@me/guilds?limit=100";
                if (!string.IsNullOrWhiteSpace(afterId))
                    route += $"&after={afterId}";

                var response = await GetApiResponseAsync(token, route);

                var isEmpty = true;

                // Get full guild object
                foreach (var guildJson in response.EnumerateArray())
                {
                    var guildId = ParseId(guildJson);

                    yield return await GetGuildAsync(token, guildId);
                    afterId = guildId;

                    isEmpty = false;
                }

                if (isEmpty)
                    yield break;
            }
        }

        public async Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(AuthToken token)
        {
            var response = await GetApiResponseAsync(token, "users/@me/channels");
            var channels = response.EnumerateArray().Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(AuthToken token, string guildId)
        {
            // Special case for direct messages pseudo-guild
            if (guildId == Guild.DirectMessages.Id)
                return Array.Empty<Channel>();

            var response = await GetApiResponseAsync(token, $"guilds/{guildId}/channels");
            var channels = response.EnumerateArray().Select(ParseChannel).ToArray();

            return channels;
        }

        private async Task<Message> GetLastMessageAsync(AuthToken token, string channelId, DateTimeOffset? before = null)
        {
            var route = $"channels/{channelId}/messages?limit=1";
            if (before != null)
                route += $"&before={before.Value.ToSnowflake()}";

            var response = await GetApiResponseAsync(token, route);

            return response.EnumerateArray().Select(ParseMessage).FirstOrDefault();
        }

        public async IAsyncEnumerable<Message> GetMessagesAsync(AuthToken token, string channelId,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double>? progress = null)
        {
            // Get the last message
            var lastMessage = await GetLastMessageAsync(token, channelId, before);

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
                var response = await GetApiResponseAsync(token, route);

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

        public void Dispose() => _httpClient.Dispose();
    }
}