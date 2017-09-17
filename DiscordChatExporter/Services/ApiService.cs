using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordChatExporter.Models;
using Newtonsoft.Json.Linq;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Services
{
    public partial class ApiService : IApiService, IDisposable
    {
        private const string ApiRoot = "https://discordapp.com/api";
        private readonly HttpClient _httpClient = new HttpClient();

        ~ApiService()
        {
            Dispose(false);
        }

        public async Task<IEnumerable<Guild>> GetGuildsAsync(string token)
        {
            // Form request url
            var url = $"{ApiRoot}/users/@me/guilds?token={token}&limit=100";

            // Get response
            var response = await _httpClient.GetStringAsync(url);

            // Parse
            var guilds = ParseGuilds(response);

            return guilds;
        }

        public async Task<IEnumerable<Channel>> GetDirectMessageChannelsAsync(string token)
        {
            // Form request url
            var url = $"{ApiRoot}/users/@me/channels?token={token}";

            // Get response
            var response = await _httpClient.GetStringAsync(url);

            // Parse
            var channels = ParseChannels(response);

            return channels;
        }

        public async Task<IEnumerable<Channel>> GetGuildChannelsAsync(string token, string guildId)
        {
            // Form request url
            var url = $"{ApiRoot}/guilds/{guildId}/channels?token={token}";

            // Get response
            var response = await _httpClient.GetStringAsync(url);

            // Parse
            var channels = ParseChannels(response);

            return channels;
        }

        public async Task<IEnumerable<Message>> GetChannelMessagesAsync(string token, string channelId)
        {
            var result = new List<Message>();

            // We are going backwards from last message to first
            // collecting everything between them in batches
            string beforeId = null;
            while (true)
            {
                // Form request url
                var url = $"{ApiRoot}/channels/{channelId}/messages?token={token}&limit=100";
                if (beforeId.IsNotBlank())
                    url += $"&before={beforeId}";

                // Get response
                var response = await _httpClient.GetStringAsync(url);

                // Parse
                var messages = ParseMessages(response);

                // Add messages to list
                string currentMessageId = null;
                foreach (var message in messages)
                {
                    result.Add(message);
                    currentMessageId = message.Id;
                }

                // If no messages - break
                if (currentMessageId == null) break;

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

    public partial class ApiService
    {
        private static IEnumerable<Guild> ParseGuilds(string json)
        {
            foreach (var guildJson in JArray.Parse(json))
            {
                // Get basic data
                var id = guildJson.Value<string>("id");
                var name = guildJson.Value<string>("name");

                var guild = new Guild(id, name);

                yield return guild;
            }
        }

        private static IEnumerable<Channel> ParseChannels(string json)
        {
            foreach (var channelJson in JArray.Parse(json))
            {
                // Get basic data
                var id = channelJson.Value<string>("id");
                var name = channelJson.Value<string>("name") ?? channelJson["recipient"].Value<string>("username");
                var type = channelJson.Value<string>("type")?.ToLowerInvariant();

                // Skip non-text channels
                if (!type.IsEither(null, "text", "dm", "group_dm"))
                    continue;

                var channel = new Channel(id, name);

                yield return channel;
            }
        }

        private static IEnumerable<Message> ParseMessages(string json)
        {
            foreach (var messageJson in JArray.Parse(json))
            {
                // Get basic data
                var id = messageJson.Value<string>("id");
                var timeStamp = messageJson.Value<DateTime>("timestamp");
                var editedTimeStamp = messageJson.Value<DateTime?>("edited_timestamp");
                var content = messageJson.Value<string>("content");

                // Lazy workaround for calls
                if (messageJson["call"] != null)
                    content = "Started a call.";

                // Get author
                var authorJson = messageJson["author"];
                var authorId = authorJson.Value<string>("id");
                var authorName = authorJson.Value<string>("username");
                var authorAvatarHash = authorJson.Value<string>("avatar");

                // Get attachment
                var attachments = new List<Attachment>();
                foreach (var attachmentJson in messageJson["attachments"].EmptyIfNull())
                {
                    var attachmentId = attachmentJson.Value<string>("id");
                    var attachmentUrl = attachmentJson.Value<string>("url");
                    var attachmentFileName = attachmentJson.Value<string>("filename");
                    var attachmentIsImage = attachmentJson["width"] != null;

                    var attachment = new Attachment(attachmentId, attachmentUrl, attachmentFileName, attachmentIsImage);
                    attachments.Add(attachment);
                }

                var author = new User(authorId, authorName, authorAvatarHash);
                var message = new Message(id, timeStamp, editedTimeStamp, author, content, attachments);

                yield return message;
            }
        }
    }
}