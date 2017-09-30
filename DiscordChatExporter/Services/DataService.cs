using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        public async Task<IReadOnlyList<Guild>> GetGuildsAsync(string token)
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

        public async Task<IReadOnlyList<Message>> GetChannelMessagesAsync(string token, string channelId, DateTime? from, DateTime? to)
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
                var content = await GetStringAsync(url);

                // Parse
                var messages = JArray.Parse(content).Select(ParseMessage);

                // Add messages to list
                var currentMessageId = default(string);
                foreach (var message in messages)
                {
                    result.Add(message);
                    currentMessageId = message.Id;
                }

                // If no messages - break
                if (currentMessageId == null) break;

                // If last message is older than from date - break
                if (from != null && result.Last().TimeStamp < from) break;

                // Otherwise offset the next request
                beforeId = currentMessageId;
            }

            // Messages appear newest first, we need to reverse
            result.Reverse();

            // Filter
            if (from != null)
                result.RemoveAll(m => m.TimeStamp < from);
            if (to != null)
                result.RemoveAll(m => m.TimeStamp > to);

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
        private static User ParseUser(JToken token)
        {
            var id = token.Value<string>("id");
            var discriminator = token.Value<int>("discriminator");
            var name = token.Value<string>("username");
            var avatarHash = token.Value<string>("avatar");

            return new User(id, discriminator, name, avatarHash);
        }

        private static Guild ParseGuild(JToken token)
        {
            var id = token.Value<string>("id");
            var name = token.Value<string>("name");
            var iconHash = token.Value<string>("icon");

            return new Guild(id, name, iconHash);
        }

        private static Channel ParseChannel(JToken token)
        {
            // Get basic data
            var id = token.Value<string>("id");
            var type = (ChannelType) token.Value<int>("type");

            // Extract name based on type
            string name;
            if (type.IsEither(ChannelType.DirectTextChat, ChannelType.DirectGroupTextChat))
            {
                var recipients = token["recipients"].Select(ParseUser);
                name = recipients.Select(r => r.Name).JoinToString(", ");
            }
            else
            {
                name = token.Value<string>("name");
            }

            return new Channel(id, name, type);
        }

        private static Message ParseMessage(JToken token)
        {
            // Get basic data
            var id = token.Value<string>("id");
            var timeStamp = token.Value<DateTime>("timestamp");
            var editedTimeStamp = token.Value<DateTime?>("edited_timestamp");
            var content = token.Value<string>("content");

            // Lazy workaround for calls
            if (token["call"] != null)
                content = "Started a call.";

            // Get author
            var author = ParseUser(token["author"]);

            // Get attachment
            var attachments = new List<Attachment>();
            foreach (var attachmentJson in token["attachments"].EmptyIfNull())
            {
                var attachmentId = attachmentJson.Value<string>("id");
                var attachmentUrl = attachmentJson.Value<string>("url");
                var attachmentType = attachmentJson["width"] != null
                    ? AttachmentType.Image
                    : AttachmentType.Other;
                var attachmentFileName = attachmentJson.Value<string>("filename");
                var attachmentFileSize = attachmentJson.Value<long>("size");

                var attachment = new Attachment(
                    attachmentId, attachmentType, attachmentUrl,
                    attachmentFileName, attachmentFileSize);
                attachments.Add(attachment);
            }

            return new Message(id, author, timeStamp, editedTimeStamp, content, attachments);
        }
    }
}