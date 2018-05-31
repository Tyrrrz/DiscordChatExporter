using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Models;
using Newtonsoft.Json.Linq;
using Tyrrrz.Extensions;
using System.Drawing;
using System.Numerics;
using DiscordChatExporter.Core.Models.Embeds;

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService : IDataService, IDisposable
    {
        private const string ApiRoot = "https://discordapp.com/api/v6";

        private readonly HttpClient _httpClient = new HttpClient();

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

            return new Guild(id, name, iconHash);
        }

        private Channel ParseChannel(JToken token)
        {
            // Get basic data
            var id = token["id"].Value<string>();
            var type = (ChannelType) token["type"].Value<int>();
            var topic = token["topic"]?.Value<string>();

            // Try to extract guild ID
            var guildId = token["guild_id"]?.Value<string>();

            // If the guild ID is blank, it's direct messages
            if (guildId.IsBlank())
                guildId = Guild.DirectMessages.Id;

            // Try to extract name
            var name = token["name"]?.Value<string>();

            // If the name is blank, it's direct messages
            if (name.IsBlank())
                name = token["recipients"].Select(ParseUser).Select(u => u.Name).JoinToString(", ");

            return new Channel(id, guildId, name, topic, type);
        }

        private Embed ParseEmbed(JToken token)
        {

            // var embedFileSize = embedJson["size"].Value<long>();
            var title = token["title"]?.Value<string>();
            var type = token["type"]?.Value<string>();
            var description = token["description"]?.Value<string>();
            var url = token["url"]?.Value<string>();
            var timestamp = token["timestamp"]?.Value<DateTime>();
            var color = token["color"] != null
                ? Color.FromArgb(token["color"].Value<int>())
                : (Color?)null;

            var footerNode = token["footer"];
            var footer = footerNode != null
                ? new EmbedFooter(
                    footerNode["text"]?.Value<string>(),
                    footerNode["icon_url"]?.Value<string>(),
                    footerNode["proxy_icon_url"]?.Value<string>())
                : null;

            var imageNode = token["image"];
            var image = imageNode != null
                ? new EmbedImage(
                    imageNode["url"]?.Value<string>(),
                    imageNode["proxy_url"]?.Value<string>(),
                    imageNode["height"]?.Value<int>(),
                    imageNode["width"]?.Value<int>())
                : null;

            var thumbnailNode = token["thumbnail"];
            var thumbnail = thumbnailNode != null
                ? new EmbedImage(
                    thumbnailNode["url"]?.Value<string>(),
                    thumbnailNode["proxy_url"]?.Value<string>(),
                    thumbnailNode["height"]?.Value<int>(),
                    thumbnailNode["width"]?.Value<int>())
                : null;

            var videoNode = token["video"];
            var video = videoNode != null
                ? new EmbedVideo(
                    videoNode["url"]?.Value<string>(),
                    videoNode["height"]?.Value<int>(),
                    videoNode["width"]?.Value<int>())
                : null;

            var providerNode = token["provider"];
            var provider = providerNode != null
                ? new EmbedProvider(
                    providerNode["name"]?.Value<string>(),
                    providerNode["url"]?.Value<string>())
                : null;

            var authorNode = token["author"];
            var author = authorNode != null
                ? new EmbedAuthor(
                    authorNode["name"]?.Value<string>(),
                    authorNode["url"]?.Value<string>(),
                    authorNode["icon_url"]?.Value<string>(),
                    authorNode["proxy_icon_url"]?.Value<string>())
                : null;

            var fields = new List<EmbedField>();
            foreach (var fieldNode in token["fields"].EmptyIfNull())
            {
                fields.Add(new EmbedField(
                    fieldNode["name"]?.Value<string>(),
                    fieldNode["value"]?.Value<string>(),
                    fieldNode["inline"]?.Value<bool>()));
            }

            var mentionableContent = description ?? "";
            fields.ForEach(f => mentionableContent += f.Value);

            // Get user mentions
            var mentionedUsers = Regex.Matches(mentionableContent, "<@!?(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => User.CreateUnknownUser(i))
                .ToList();

            // Get channel mentions
            var mentionedChannels = Regex.Matches(mentionableContent, "<#(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => Channel.CreateDeletedChannel(i))
                .ToList();

            // Get role mentions
            var mentionedRoles = Regex.Matches(mentionableContent, "<@&(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => Role.CreateDeletedRole(i))
                .ToList();

            var mentions = new MentionContainer(mentionedUsers, mentionedChannels, mentionedRoles);

            return new Embed(title, type, description, url, timestamp, color, footer, image, thumbnail, video, provider,
                author, fields, mentions);
        }

        private Message ParseMessage(JToken token)
        {
            // Get basic data
            var id = token["id"].Value<string>();
            var channelId = token["channel_id"].Value<string>();
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
                var attachmentIsImage = attachmentJson["width"] != null;
                var attachmentFileName = attachmentJson["filename"].Value<string>();
                var attachmentFileSize = attachmentJson["size"].Value<long>();

                var attachment = new Attachment(
                    attachmentId, attachmentIsImage, attachmentUrl,
                    attachmentFileName, attachmentFileSize);
                attachments.Add(attachment);
            }

            // Get embeds
            var embeds = token["embeds"].EmptyIfNull().Select(ParseEmbed).ToArray();

            // Get user mentions
            var mentionedUsers = token["mentions"].Select(ParseUser).ToArray();

            // Get channel mentions
            var mentionedChannels = Regex.Matches(content, "<#(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => Channel.CreateDeletedChannel(i))
                .ToList();

            // Get role mentions
            var mentionedRoles = token["mention_roles"]
                .Values<string>()
                .Select(i => Role.CreateDeletedRole(i))
                .ToList();

            var mentions = new MentionContainer(mentionedUsers, mentionedChannels, mentionedRoles);

            return new Message(id, channelId, type, author, timeStamp, editedTimeStamp, content, attachments, embeds,
                mentions);
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

            return guild;
        }

        public async Task<Channel> GetChannelAsync(string token, string channelId)
        {
            // Form request url
            var url = $"{ApiRoot}/channels/{channelId}?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var channel = ParseChannel(JToken.Parse(content));

            return channel;
        }

        public async Task<User> GetMemberAsync(string token, string guildId, string memberId)
        {
            // Form request url
            var url = $"{ApiRoot}/guilds/{guildId}/members/{memberId}?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var user = ParseUser(JToken.Parse(content)["user"]);

            return user;
        }

        public async Task<IReadOnlyList<Channel>> GetGuildChannelsAsync(string token, string guildId)
        {
            // Form request url
            var url = $"{ApiRoot}/guilds/{guildId}/channels?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var channels = JArray.Parse(content).Select(ParseChannel).ToArray();

            return channels;
        }


        public async Task<IReadOnlyList<Role>> GetGuildRolesAsync(string token, string guildId)
        {
            // Form request url
            var url = $"{ApiRoot}/guilds/{guildId}/roles?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var roles = JArray.Parse(content).Select(ParseRole).ToArray();

            return roles;
        }

        public async Task<IReadOnlyList<Guild>> GetUserGuildsAsync(string token)
        {
            // Form request url
            var url = $"{ApiRoot}/users/@me/guilds?token={token}&limit=100";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var guilds = JArray.Parse(content).Select(ParseGuild).ToArray();

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

        public async Task<IReadOnlyList<User>> GetGuildMembersAsync(string token, string guildId)
        {
            var result = new List<User>();
            
            var afterId = "";
            while (true)
            {
                // Form request url
                var url = $"{ApiRoot}/guilds/{guildId}/members?token={token}&limit=1000";
                if (afterId.IsNotBlank())
                    url += $"&after={afterId}";

                // Get response
                var content = await GetStringAsync(url);

                // Parse
                var users = JArray.Parse(content).Select(m => ParseUser(m["user"]));

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