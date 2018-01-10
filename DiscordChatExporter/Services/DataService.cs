using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Exceptions;
using DiscordChatExporter.Models;
using Newtonsoft.Json.Linq;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Services
{
    public partial class DataService : IDataService, IDisposable
    {
        private const string ApiRoot = "https://discordapp.com/api/v6";

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Dictionary<string, Role> _roleCache = new Dictionary<string, Role>();
        private readonly Dictionary<string, Channel> _channelCache = new Dictionary<string, Channel>();

        private User ParseUser(JToken token)
        {
            var id = token["id"].Value<string>();
            var discriminator = token["discriminator"].Value<int>();
            var name = token["username"].Value<string>();
            var avatarHash = token["avatar"].Value<string>();

            return new User(id, discriminator, name, avatarHash);
        }

        private Role ParseRole(JToken token)
        {
            var id = token["id"].Value<string>();
            var name = token["name"].Value<string>();

            return new Role(id, name);
        }

        private Guild ParseGuild(JToken token)
        {
            var id = token["id"].Value<string>();
            var name = token["name"].Value<string>();
            var iconHash = token["icon"].Value<string>();
            var roles = token["roles"].Select(ParseRole).ToArray();

            return new Guild(id, name, iconHash, roles);
        }

        private Channel ParseChannel(JToken token)
        {
            // Get basic data
            var id = token["id"].Value<string>();
            var type = (ChannelType) token["type"].Value<int>();
            var topic = token["topic"]?.Value<string>();

            // Extract name based on type
            string name;
            if (type.IsEither(ChannelType.DirectTextChat, ChannelType.DirectGroupTextChat))
            {
                var recipients = token["recipients"].Select(ParseUser);
                name = recipients.Select(r => r.Name).JoinToString(", ");
            }
            else
            {
                name = token["name"].Value<string>();
            }

            return new Channel(id, name, topic, type);
        }

        private Message ParseMessage(JToken token)
        {
            // Get basic data
            var id = token["id"].Value<string>();
            var timeStamp = token["timestamp"].Value<DateTime>();
            var editedTimeStamp = token["edited_timestamp"]?.Value<DateTime?>();
            var content = token["content"].Value<string>();
            var type = (MessageType) token["type"].Value<int>();

            // Workarounds for non-default types
            if (type == MessageType.RecipientAdd)
                content = "Added a recipient.";
            else if (type == MessageType.RecipientRemove)
                content = "Removed a recipient.";
            else if (type == MessageType.Call)
                content = "Started a call.";
            else if (type == MessageType.ChannelNameChange)
                content = "Changed the channel name.";
            else if (type == MessageType.ChannelIconChange)
                content = "Changed the channel icon.";
            else if (type == MessageType.ChannelPinnedMessage)
                content = "Pinned a message.";
            else if (type == MessageType.GuildMemberJoin)
                content = "Joined the server.";

            // Get author
            var author = ParseUser(token["author"]);

            // Get attachment
            var attachments = new List<Attachment>();
            foreach (var attachmentJson in token["attachments"].EmptyIfNull())
            {
                var attachmentId = attachmentJson["id"].Value<string>();
                var attachmentUrl = attachmentJson["url"].Value<string>();
                var attachmentType = attachmentJson["width"] != null
                    ? AttachmentType.Image
                    : AttachmentType.Other;
                var attachmentFileName = attachmentJson["filename"].Value<string>();
                var attachmentFileSize = attachmentJson["size"].Value<long>();

                var attachment = new Attachment(
                    attachmentId, attachmentType, attachmentUrl,
                    attachmentFileName, attachmentFileSize);
                attachments.Add(attachment);
            }

            // Get user mentions
            var mentionedUsers = token["mentions"].Select(ParseUser).ToArray();

            // Get role mentions
            var mentionedRoles = token["mention_roles"]
                .Values<string>()
                .Select(i => _roleCache.GetOrDefault(i) ?? Role.CreateDeletedRole(id))
                .ToArray();

            // Get channel mentions
            var mentionedChannels = Regex.Matches(content, "<#(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => _channelCache.GetOrDefault(i) ?? Channel.CreateDeletedChannel(id))
                .ToArray();

            return new Message(id, type, author, timeStamp, editedTimeStamp, content, attachments,
                mentionedUsers, mentionedRoles, mentionedChannels);
        }

        private async Task<string> GetStringAsync(string url)
        {
            using (var response = await _httpClient.GetAsync(url))
            {
                // Check status code
                // We throw our own exception here because default one doesn't have status code
                if (!response.IsSuccessStatusCode)
                    throw new HttpErrorStatusCodeException(response.StatusCode);

                // Get content
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<Guild> GetGuildAsync(string token, string guildId)
        {
            // Form request url
            var url = $"{ApiRoot}/guilds/{guildId}?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var guild = ParseGuild(JToken.Parse(content));

            // Add roles to cache
            foreach (var role in guild.Roles)
                _roleCache[role.Id] = role;

            return guild;
        }

        public async Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(string token, string guildId)
        {
            // Form request url
            var url = $"{ApiRoot}/guilds/{guildId}/channels?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var channels = JArray.Parse(content).Select(ParseChannel).ToArray();

            // Add channels to cache
            foreach (var channel in channels)
                _channelCache[channel.Id] = channel;

            return channels;
        }

        public async Task<IReadOnlyList<Guild>> GetUserGuildsAsync(string token)
        {
            // Form request url
            var url = $"{ApiRoot}/users/@me/guilds?token={token}&limit=100";

            // Get response
            var content = await GetStringAsync(url);

            // Parse IDs
            var guildIds = JArray.Parse(content).Select(t => t["id"].Value<string>());

            // Get full guild infos
            var guilds = new List<Guild>();
            foreach (var guildId in guildIds)
            {
                var guild = await GetGuildAsync(token, guildId);
                guilds.Add(guild);
            }

            return guilds;
        }

        public async Task<IReadOnlyList<Channel>> GetDirectMessageChannelsAsync(string token)
        {
            // Form request url
            var url = $"{ApiRoot}/users/@me/channels?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var channels = JArray.Parse(content).Select(ParseChannel).ToArray();

            return channels;
        }

        public async Task<IReadOnlyList<Message>> GetChannelMessagesAsync(string token, string channelId,
            DateTime? from, DateTime? to)
        {
            var result = new List<Message>();

            // We are going backwards from last message to first
            // collecting everything between them in batches
            var beforeId = to != null ? DateTimeToSnowflake(to.Value) : null;
            while (true)
            {
                // Form request url
                var url = $"{ApiRoot}/channels/{channelId}/messages?token={token}&limit=100";
                if (beforeId.IsNotBlank())
                    url += $"&before={beforeId}";

                // Get response
                var content = await GetStringAsync(url);

                // Parse
                var messages = JArray.Parse(content).Select(ParseMessage);

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

    public partial class DataService
    {
        private static string DateTimeToSnowflake(DateTime dateTime)
        {
            const long epoch = 62135596800000;
            var unixTime = dateTime.ToUniversalTime().Ticks / TimeSpan.TicksPerMillisecond - epoch;
            var value = ((ulong) unixTime - 1420070400000UL) << 22;
            return value.ToString();
        }
    }
}