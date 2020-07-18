﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exceptions;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;
using Polly;

namespace DiscordChatExporter.Domain.Discord
{
    public class DiscordClient
    {
        private readonly AuthToken _token;
        private readonly HttpClient _httpClient = Singleton.HttpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _httpRequestPolicy;

        private readonly Uri _baseUri = new Uri("https://discordapp.com/api/v6/", UriKind.Absolute);

        public DiscordClient(AuthToken token)
        {
            _token = token;

            // Discord seems to always respond with 429 on the first request with unreasonable wait time (10+ minutes).
            // For that reason the policy will ignore such errors at first, then wait a constant amount of time, and
            // finally wait the specified amount of time, based on how many requests have failed in a row.
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
                    (response, timespan, retryCount, context) => Task.CompletedTask
                );
        }

        private async Task<HttpResponseMessage> GetResponseAsync(string url) => await _httpRequestPolicy.ExecuteAsync(async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, url));
            request.Headers.Authorization = _token.GetAuthorizationHeader();

            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        });

        private async Task<JsonElement> GetJsonResponseAsync(string url)
        {
            using var response = await GetResponseAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => DiscordChatExporterException.Unauthorized(),
                    HttpStatusCode.Forbidden => DiscordChatExporterException.Forbidden(),
                    HttpStatusCode.NotFound => DiscordChatExporterException.NotFound(),
                    _ => DiscordChatExporterException.FailedHttpRequest(response)
                };
            }

            return await response.Content.ReadAsJsonAsync();
        }

        private async Task<JsonElement?> TryGetJsonResponseAsync(string url)
        {
            using var response = await GetResponseAsync(url);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsJsonAsync()
                : (JsonElement?) null;
        }

        public async IAsyncEnumerable<Guild> GetUserGuildsAsync()
        {
            yield return Guild.DirectMessages;

            var afterId = "";
            while (true)
            {
                var url = new UrlBuilder()
                    .SetPath("users/@me/guilds")
                    .SetQueryParameter("limit", "100")
                    .SetQueryParameter("after", afterId)
                    .Build();

                var response = await GetJsonResponseAsync(url);

                var isEmpty = true;
                foreach (var guild in response.EnumerateArray().Select(Guild.Parse))
                {
                    yield return guild;

                    afterId = guild.Id;
                    isEmpty = false;
                }

                if (isEmpty)
                    yield break;
            }
        }

        public async Task<Guild> GetGuildAsync(string guildId)
        {
            if (guildId == Guild.DirectMessages.Id)
                return Guild.DirectMessages;

            var response = await GetJsonResponseAsync($"guilds/{guildId}");
            return Guild.Parse(response);
        }

        public async IAsyncEnumerable<Channel> GetGuildChannelsAsync(string guildId)
        {
            if (guildId == Guild.DirectMessages.Id)
            {
                var response = await GetJsonResponseAsync("users/@me/channels");
                foreach (var channelJson in response.EnumerateArray())
                    yield return Channel.Parse(channelJson);
            }
            else
            {
                var response = await GetJsonResponseAsync($"guilds/{guildId}/channels");

                var categories = response
                    .EnumerateArray()
                    .ToDictionary(
                        j => j.GetProperty("id").GetString(),
                        j => j.GetProperty("name").GetString()
                    );

                foreach (var channelJson in response.EnumerateArray())
                {
                    var parentId = channelJson.GetPropertyOrNull("parent_id")?.GetString();
                    var category = !string.IsNullOrWhiteSpace(parentId)
                        ? categories.GetValueOrDefault(parentId)
                        : null;

                    var channel = Channel.Parse(channelJson, category);

                    // Skip non-text channels
                    if (!channel.IsTextChannel)
                        continue;

                    yield return channel;
                }
            }
        }

        public async IAsyncEnumerable<Role> GetGuildRolesAsync(string guildId)
        {
            if (guildId == Guild.DirectMessages.Id)
                yield break;

            var response = await GetJsonResponseAsync($"guilds/{guildId}/roles");

            foreach (var roleJson in response.EnumerateArray())
                yield return Role.Parse(roleJson);
        }

        public async Task<Member?> TryGetGuildMemberAsync(string guildId, User user)
        {
            if (guildId == Guild.DirectMessages.Id)
                return Member.CreateForUser(user);

            var response = await TryGetJsonResponseAsync($"guilds/{guildId}/members/{user.Id}");
            return response?.Pipe(Member.Parse);
        }

        private async Task<string> GetChannelCategoryAsync(string channelParentId)
        {
            var response = await GetJsonResponseAsync($"channels/{channelParentId}");
            return response.GetProperty("name").GetString();
        }

        public async Task<Channel> GetChannelAsync(string channelId)
        {
            var response = await GetJsonResponseAsync($"channels/{channelId}");

            var parentId = response.GetPropertyOrNull("parent_id")?.GetString();
            var category = !string.IsNullOrWhiteSpace(parentId)
                ? await GetChannelCategoryAsync(parentId)
                : null;

            return Channel.Parse(response, category);
        }

        private async Task<Message?> TryGetLastMessageAsync(string channelId, DateTimeOffset? before = null)
        {
            var url = new UrlBuilder()
                .SetPath($"channels/{channelId}/messages")
                .SetQueryParameter("limit", "1")
                .SetQueryParameter("before", before?.ToSnowflake())
                .Build();

            var response = await GetJsonResponseAsync(url);
            return response.EnumerateArray().Select(Message.Parse).LastOrDefault();
        }

        public async IAsyncEnumerable<Message> GetMessagesAsync(
            string channelId,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null,
            IProgress<double>? progress = null)
        {
            // Get the last message in the specified range.
            // This snapshots the boundaries, which means that messages posted after the exported started
            // will not appear in the output.
            // Additionally, it provides the date of the last message, which is used to calculate progress.
            var lastMessage = await TryGetLastMessageAsync(channelId, before);
            if (lastMessage == null || lastMessage.Timestamp < after)
                yield break;

            // Keep track of first message in range in order to calculate progress
            var firstMessage = default(Message);
            var afterId = after?.ToSnowflake() ?? "0";

            while (true)
            {
                var url = new UrlBuilder()
                    .SetPath($"channels/{channelId}/messages")
                    .SetQueryParameter("limit", "100")
                    .SetQueryParameter("after", afterId)
                    .Build();

                var response = await GetJsonResponseAsync(url);

                var messages = response
                    .EnumerateArray()
                    .Select(Message.Parse)
                    .Reverse() // reverse because messages appear newest first
                    .ToArray();

                // Break if there are no messages (can happen if messages are deleted during execution)
                if (!messages.Any())
                    yield break;

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
                }
            }
        }
    }
}