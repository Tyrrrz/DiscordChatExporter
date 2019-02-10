using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Models;
using Newtonsoft.Json.Linq;
using DiscordChatExporter.Core.Internal;
using Failsafe;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService : IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private async Task<JToken> GetApiResponseAsync(AuthToken token, string resource, string endpoint,
            params string[] parameters)
        {
            // Create retry policy
            var retry = Retry.Create()
                .Catch<HttpErrorStatusCodeException>(false, e => e.StatusCode == HttpStatusCode.ServiceUnavailable)
                .Catch<HttpErrorStatusCodeException>(false, e => (int) e.StatusCode == 429)
                .WithMaxTryCount(10)
                .WithDelay(TimeSpan.FromSeconds(0.4));

            // Send request
            return await retry.ExecuteAsync(async () =>
            {
                // Create request
                const string apiRoot = "https://discordapp.com/api/v6";
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{apiRoot}/{resource}/{endpoint}"))
                {
                    // Set authorization header
                    request.Headers.Authorization = token.Type == AuthTokenType.Bot
                        ? new AuthenticationHeaderValue("Bot", token.Value)
                        : new AuthenticationHeaderValue(token.Value);

                    // Add parameters
                    foreach (var parameter in parameters.ExceptBlank())
                    {
                        var key = parameter.SubstringUntil("=");
                        var value = parameter.SubstringAfter("=");

                        // Skip empty values
                        if (value.IsBlank())
                            continue;

                        request.RequestUri = request.RequestUri.SetQueryParameter(key, value);
                    }

                    // Get response
                    using (var response = await _httpClient.SendAsync(request))
                    {
                        // Check status code
                        // We throw our own exception here because default one doesn't have status code
                        if (!response.IsSuccessStatusCode)
                            throw new HttpErrorStatusCodeException(response.StatusCode, response.ReasonPhrase);

                        // Get content
                        var raw = await response.Content.ReadAsStringAsync();

                        // Parse
                        return JToken.Parse(raw);
                    }
                }
            });
        }

        public async Task<Guild> GetGuildAsync(AuthToken token, string guildId)
        {
            // Special case for direct messages pseudo-guild
            if (guildId == Guild.DirectMessages.Id)
                return Guild.DirectMessages;

            var response = await GetApiResponseAsync(token, "guilds", guildId);
            var guild = ParseGuild(response);

            return guild;
        }

        public async Task<Channel> GetChannelAsync(AuthToken token, string channelId)
        {
            var response = await GetApiResponseAsync(token, "channels", channelId);
            var channel = ParseChannel(response);

            return channel;
        }

        public async Task<IReadOnlyList<Guild>> GetUserGuildsAsync(AuthToken token)
        {
            var response = await GetApiResponseAsync(token, "users", "@me/guilds", "limit=100");
            var guilds = response.Select(ParseGuild).ToArray();

            return guilds;
        }

        public async Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(AuthToken token)
        {
            var response = await GetApiResponseAsync(token, "users", "@me/channels");
            var channels = response.Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(AuthToken token, string guildId)
        {
            var response = await GetApiResponseAsync(token, "guilds", $"{guildId}/channels");
            var channels = response.Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Role>> GetGuildRolesAsync(AuthToken token, string guildId)
        {
            var response = await GetApiResponseAsync(token, "guilds", $"{guildId}/roles");
            var roles = response.Select(ParseRole).ToArray();

            return roles;
        }

        public async Task<IReadOnlyList<Message>> GetChannelMessagesAsync(AuthToken token, string channelId,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null)
        {
            var result = new List<Message>();

            // Get the last message
            var response = await GetApiResponseAsync(token, "channels", $"{channelId}/messages",
                "limit=1", $"before={to?.ToSnowflake()}");
            var lastMessage = response.Select(ParseMessage).FirstOrDefault();

            // If the last message doesn't exist or it's outside of range - return
            if (lastMessage == null || lastMessage.Timestamp < from)
            {
                progress?.Report(1);
                return result;
            }

            // Get other messages
            var offsetId = from?.ToSnowflake() ?? "0";
            while (true)
            {
                // Get message batch
                response = await GetApiResponseAsync(token, "channels", $"{channelId}/messages",
                    "limit=100", $"after={offsetId}");

                // Parse
                var messages = response
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

                // Add to result
                result.AddRange(messagesInRange);

                // Break if messages were trimmed (which means the last message was encountered)
                if (messagesInRange.Length != messages.Length)
                    break;

                // Report progress (based on the time range of parsed messages compared to total)
                progress?.Report((result.Last().Timestamp - result.First().Timestamp).TotalSeconds /
                                 (lastMessage.Timestamp - result.First().Timestamp).TotalSeconds);

                // Move offset
                offsetId = result.Last().Id;
            }

            // Add last message
            result.Add(lastMessage);

            // Report progress
            progress?.Report(1);

            return result;
        }

        public async Task<Mentionables> GetMentionablesAsync(AuthToken token, string guildId,
            IEnumerable<Message> messages)
        {
            // Get channels and roles
            var channels = guildId != Guild.DirectMessages.Id
                ? await GetGuildChannelsAsync(token, guildId)
                : Array.Empty<Channel>();
            var roles = guildId != Guild.DirectMessages.Id
                ? await GetGuildRolesAsync(token, guildId)
                : Array.Empty<Role>();

            // Get users
            var userMap = new Dictionary<string, User>();
            foreach (var message in messages)
            {
                // Author
                userMap[message.Author.Id] = message.Author;

                // Mentioned users
                foreach (var mentionedUser in message.MentionedUsers)
                    userMap[mentionedUser.Id] = mentionedUser;
            }

            var users = userMap.Values.ToArray();

            return new Mentionables(users, channels, roles);
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, Guild guild, Channel channel,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null)
        {
            // Get messages
            var messages = await GetChannelMessagesAsync(token, channel.Id, from, to, progress);

            // Get mentionables
            var mentionables = await GetMentionablesAsync(token, guild.Id, messages);

            return new ChatLog(guild, channel, from, to, messages, mentionables);
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, Channel channel,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null)
        {
            // Get guild
            var guild = channel.GuildId == Guild.DirectMessages.Id
                ? Guild.DirectMessages
                : await GetGuildAsync(token, channel.GuildId);

            // Get the chat log
            return await GetChatLogAsync(token, guild, channel, from, to, progress);
        }

        public async Task<ChatLog> GetChatLogAsync(AuthToken token, string channelId,
            DateTime? from = null, DateTime? to = null, IProgress<double> progress = null)
        {
            // Get channel
            var channel = await GetChannelAsync(token, channelId);

            // Get the chat log
            return await GetChatLogAsync(token, channel, from, to, progress);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}