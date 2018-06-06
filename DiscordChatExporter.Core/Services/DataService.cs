using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, User> _userCache;
        private readonly Dictionary<string, Channel> _channelCache;
        private readonly Dictionary<string, Role> _roleCache;

        public DataService()
        {
            _httpClient = new HttpClient();
            _userCache = new Dictionary<string, User>();
            _channelCache = new Dictionary<string, Channel>();
            _roleCache = new Dictionary<string, Role>();
        }

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

        private Attachment ParseAttachment(JToken token)
        {
            var id = token["id"].Value<string>();
            var url = token["url"].Value<string>();
            var isImage = token["width"] != null;
            var fileName = token["filename"].Value<string>();
            var fileSize = token["size"].Value<long>();

            return new Attachment(id, isImage, url, fileName, fileSize);
        }

        private EmbedFooter ParseEmbedFooter(JToken token)
        {
            var text = token["text"]?.Value<string>();
            var iconUrl = token["icon_url"]?.Value<string>();
            var proxyIconUrl = token["proxy_icon_url"]?.Value<string>();

            return new EmbedFooter(text, iconUrl, proxyIconUrl);
        }

        private EmbedImage ParseEmbedImage(JToken token)
        {
            var url = token["url"]?.Value<string>();
            var proxyUrl = token["proxy_url"]?.Value<string>();
            var height = token["height"]?.Value<int>();
            var width = token["width"]?.Value<int>();

            return new EmbedImage(url, proxyUrl, height, width);
        }

        private EmbedVideo ParseEmbedVideo(JToken token)
        {
            var url = token["url"]?.Value<string>();
            var height = token["height"]?.Value<int>();
            var width = token["width"]?.Value<int>();

            return new EmbedVideo(url, height, width);
        }

        private EmbedProvider ParseEmbedProvider(JToken token)
        {
            var name = token["name"]?.Value<string>();
            var url = token["url"]?.Value<string>();

            return new EmbedProvider(name, url);
        }

        private EmbedAuthor ParseEmbedAuthor(JToken token)
        {
            var name = token["name"]?.Value<string>();
            var url = token["url"]?.Value<string>();
            var iconUrl = token["icon_url"]?.Value<string>();
            var proxyIconUrl = token["proxy_icon_url"]?.Value<string>();

            return new EmbedAuthor(name, url, iconUrl, proxyIconUrl);
        }

        private EmbedField ParseEmbedField(JToken token)
        {
            var name = token["name"]?.Value<string>();
            var value = token["value"]?.Value<string>();
            var isInline = token["inline"]?.Value<bool>() ?? false;

            return new EmbedField(name, value, isInline);
        }

        private Embed ParseEmbed(JToken token)
        {
            // Get basic data
            var title = token["title"]?.Value<string>();
            var type = token["type"]?.Value<string>();
            var description = token["description"]?.Value<string>();
            var url = token["url"]?.Value<string>();
            var timestamp = token["timestamp"]?.Value<DateTime>();
            var color = token["color"] != null
                ? Color.FromArgb(token["color"].Value<int>())
                : (Color?)null;

            // Set color alpha to 1
            if (color != null)
                color = Color.FromArgb(1, color.Value);

            // Get footer
            var footer = token["footer"] != null ? ParseEmbedFooter(token["footer"]) : null;

            // Get image
            var image = token["image"] != null ? ParseEmbedImage(token["image"]) : null;

            // Get thumbnail
            var thumbnail = token["thumbnail"] != null ? ParseEmbedImage(token["thumbnail"]) : null;

            // Get video
            var video = token["video"] != null ? ParseEmbedVideo(token["video"]) : null;

            // Get provider
            var provider = token["provider"] != null? ParseEmbedProvider(token["provider"]) : null;

            // Get author
            var author = token["author"] != null ? ParseEmbedAuthor(token["author"]) : null;

            // Get fields
            var fields = token["fields"].EmptyIfNull().Select(ParseEmbedField).ToArray();

            return new Embed(title, type, description, url, timestamp, color, footer, image, thumbnail, video, provider,
                author, fields);
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

            // Get attachments
            var attachments = token["attachments"].EmptyIfNull().Select(ParseAttachment).ToArray();

            // Get embeds
            var embeds = token["embeds"].EmptyIfNull().Select(ParseEmbed).ToArray();

            // Get user mentions and cache them
            var mentionedUsers = token["mentions"].Select(ParseUser).ToArray();
            foreach (var user in mentionedUsers)
                _userCache[user.Id] = user;

            return new Message(id, channelId, type, author, timeStamp, editedTimeStamp, content, attachments, embeds);
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