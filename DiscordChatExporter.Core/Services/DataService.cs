using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Models;
using Newtonsoft.Json.Linq;
using DiscordChatExporter.Core.Internal;

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService : IDataService, IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private async Task<JToken> GetApiResponseAsync(string token, string resource, string endpoint, params string[] parameters)
        {
            // Format URL
            const string apiRoot = "https://discordapp.com/api/v6";
            var url = $"{apiRoot}/{resource}/{endpoint}?token={token}";

            // Add parameters
            foreach (var parameter in parameters)
                url += $"&{parameter}";

            // Send request
            using (var response = await _httpClient.GetAsync(url))
            {
                // Check status code
                // We throw our own exception here because default one doesn't have status code
                if (!response.IsSuccessStatusCode)
                    throw new HttpErrorStatusCodeException(response.StatusCode);

                // Get content
                var raw = await response.Content.ReadAsStringAsync();

                // Parse
                return JToken.Parse(raw);
            }
        }

        public async Task<Guild> GetGuildAsync(string token, string guildId)
        {
            var response = await GetApiResponseAsync(token, "guilds", guildId);
            var guild = ParseGuild(response);

            return guild;
        }

        public async Task<IReadOnlyList<Role>> GetGuildRolesAsync(string token, string guildId)
        {
            var response = await GetApiResponseAsync(token, "guilds", $"{guildId}/roles");
            var roles = response.Select(ParseRole).ToArray();

            return roles;
        }

        public async Task<Channel> GetChannelAsync(string token, string channelId)
        {
            var response = await GetApiResponseAsync(token, "channels", channelId);
            var channel = ParseChannel(response);

            return channel;
        }

        public async Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(string token, string guildId)
        {
            var response = await GetApiResponseAsync(token, "guilds", $"{guildId}/channels");
            var channels = response.Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Guild>> GetUserGuildsAsync(string token)
        {
            var response = await GetApiResponseAsync(token, "users", "@me/guilds", "limit=100");
            var guilds = response.Select(ParseGuild).ToArray();

            return guilds;
        }

        public async Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(string token)
        {
            var response = await GetApiResponseAsync(token, "users", "@me/channels");
            var channels = response.Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Message>> GetChannelMessagesAsync(string token, string channelId,
            DateTime? from, DateTime? to)
        {
            var result = new List<Message>();

            // We are going backwards from last message to first
            // collecting everything between them in batches
            var beforeId = to?.ToSnowflake() ?? DateTime.MaxValue.ToSnowflake();
            while (true)
            {
                // Get response
                var response = await GetApiResponseAsync(token, "channels", $"{channelId}/messages",
                    "limit=100", $"before={beforeId}");

                // Parse
                var messages = response.Select(ParseMessage);

                // Add messages to list
                string currentMessageId = null;
                foreach (var message in messages)
                {
                    // Break when the message is older than from date
                    if (from != null && message.TimeStamp < from)
                    {
                        currentMessageId = null;
                        break;
                    }

                    // Add message
                    result.Add(message);
                    currentMessageId = message.Id;
                }

                // If no messages - break
                if (currentMessageId == null)
                    break;

                // Otherwise offset the next request
                beforeId = currentMessageId;
            }

            // Messages appear newest first, we need to reverse
            result.Reverse();

            return result;
        }

        public async Task<IReadOnlyList<User>> GetGuildMembersAsync(string token, string guildId)
        {
            var result = new List<User>();

            var afterId = "0";
            while (true)
            {
                // Get response
                var response = await GetApiResponseAsync(token, "guilds", $"{guildId}/members",
                    "limit=100", $"after={afterId}");

                // Parse
                var users = response.Select(m => ParseUser(m["user"]));

                // Add users to list
                string currentUserId = null;
                foreach (var user in users)
                {
                    // Add user
                    result.Add(user);
                    if (currentUserId == null || BigInteger.Parse(user.Id) > BigInteger.Parse(currentUserId))
                        currentUserId = user.Id;
                }

                // If no users - break
                if (currentUserId == null)
                    break;

                // Otherwise offset the next request
                afterId = currentUserId;
            }

            return result;
        }

        public async Task<MentionContainer> GetGuildMentionablesAsync(string token, string guildId)
        {
            // Get guild members
            var users = await GetGuildMembersAsync(token, guildId);

            // Get guild channels
            var channels = await GetGuildChannelsAsync(token, guildId);

            // Get guild roles
            var roles = await GetGuildRolesAsync(token, guildId);

            return new MentionContainer(users, channels, roles);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}