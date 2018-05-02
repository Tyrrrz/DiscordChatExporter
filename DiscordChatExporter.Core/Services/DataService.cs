﻿using System;
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

namespace DiscordChatExporter.Core.Services
{
    public partial class DataService : IDataService, IDisposable
    {
        private const string ApiRoot = "https://discordapp.com/api/v6";

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Dictionary<string, User> _userCache = new Dictionary<string, User>();
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
            var guildId = token["guild_id"]?.Value<string>();
            var type = (ChannelType) token["type"].Value<int>();
            var topic = token["topic"]?.Value<string>();

            // Extract name based on type
            string name;
            if (type.IsEither(ChannelType.DirectTextChat, ChannelType.DirectGroupTextChat))
            {
                guildId = Guild.DirectMessages.Id;

                // Try to get name if it's set
                name = token["name"]?.Value<string>();

                // Otherwise use recipients as the name
                if (name.IsBlank())
                    name = token["recipients"].Select(ParseUser).Select(u => u.Name).JoinToString(", ");
            }
            else
            {
                name = token["name"].Value<string>();
            }

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
                .Select(i => _userCache.GetOrDefault(i) ?? User.CreateUnknownUser(i))
                .ToList();

            // Get role mentions
            var mentionedRoles = Regex.Matches(mentionableContent, "<@&(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => _roleCache.GetOrDefault(i) ?? Role.CreateDeletedRole(i))
                .ToList();

            // Get channel mentions
            var mentionedChannels = Regex.Matches(mentionableContent, "<#(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => _channelCache.GetOrDefault(i) ?? Channel.CreateDeletedChannel(i))
                .ToList();

            return new Embed(
                title, type, description,
                url, timestamp, color,
                footer, image, thumbnail,
                video, provider, author,
                fields, mentionedUsers, mentionedRoles, mentionedChannels);
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

            // Get embeds
            var embeds = token["embeds"].EmptyIfNull().Select(ParseEmbed).ToArray();

            // Get user mentions
            var mentionedUsers = token["mentions"].Select(ParseUser).ToList();

            // Get role mentions
            var mentionedRoles = token["mention_roles"]
                .Values<string>()
                .Select(i => _roleCache.GetOrDefault(i) ?? Role.CreateDeletedRole(i))
                .ToList();

            // Get channel mentions
            var mentionedChannels = Regex.Matches(content, "<#(\\d+)>")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ExceptBlank()
                .Select(i => _channelCache.GetOrDefault(i) ?? Channel.CreateDeletedChannel(i))
                .ToList();

            return new Message(id, channelId, type, author, timeStamp, editedTimeStamp, content, attachments, embeds,
                mentionedUsers, mentionedRoles, mentionedChannels);
        }

        /// <summary>
        /// Attempts to query for users, channels, and roles if they havent been found yet, and set them in the mentionable
        /// </summary>
        private async Task FillMentionable(string token, string guildId, IMentionable mentionable)
        {
            for (int i = 0; i < mentionable.MentionedUsers.Count; i++)
            {
                var user = mentionable.MentionedUsers[i];
                if (user.Name == "Unknown" && user.Discriminator == 0)
                {
                    try
                    {
                        mentionable.MentionedUsers[i] = _userCache.GetOrDefault(user.Id) ?? (await GetMemberAsync(token, guildId, user.Id));
                    }
                    catch (HttpErrorStatusCodeException e) { } // This likely means the user doesnt exist any more, so ignore
                }
            }

            for (int i = 0; i < mentionable.MentionedChannels.Count; i++)
            {
                var channel = mentionable.MentionedChannels[i];
                if (channel.Name == "deleted-channel" && channel.GuildId == null)
                {
                    try
                    {
                        mentionable.MentionedChannels[i] = _channelCache.GetOrDefault(channel.Id) ?? (await GetChannelAsync(token, channel.Id));
                    }
                    catch (HttpErrorStatusCodeException e) { } // This likely means the user doesnt exist any more, so ignore
                }
            }

            // Roles are already gotten via GetGuildRolesAsync at the start
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

        public async Task<Channel> GetChannelAsync(string token, string channelId)
        {
            // Form request url
            var url = $"{ApiRoot}/channels/{channelId}?token={token}";

            // Get response
            var content = await GetStringAsync(url);

            // Parse
            var channel = ParseChannel(JToken.Parse(content));

            // Add channel to cache
            _channelCache[channel.Id] = channel;

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

            // Add user to cache
            _userCache[user.Id] = user;

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

            // Add channels to cache
            foreach (var channel in channels)
                _channelCache[channel.Id] = channel;

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

            // Add roles to cache
            foreach (var role in roles)
                _roleCache[role.Id] = role;

            return roles;
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

                // Add user to cache
                foreach (var user in users)
                    _userCache[user.Id] = user;

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
            Channel channel = await GetChannelAsync(token, channelId);

            try
            {
                await GetGuildRolesAsync(token, channel.GuildId);
            }
            catch (HttpErrorStatusCodeException e) { } // This will be thrown if the user doesnt have the MANAGE_ROLES permission for the guild

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

            foreach (var message in result)
            {
                await FillMentionable(token, channel.GuildId, message);
                foreach (var embed in message.Embeds)
                    await FillMentionable(token, channel.GuildId, embed);
            }

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