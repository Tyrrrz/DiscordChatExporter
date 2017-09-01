using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordChatExporter.Models;
using Newtonsoft.Json.Linq;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Services
{
    public class DiscordApiService
    {
        private const string ApiRoot = "https://discordapp.com/api";
        private readonly HttpClient _httpClient = new HttpClient();

        private IEnumerable<Message> ParseMessages(string json)
        {
            var messagesJson = JArray.Parse(json);
            foreach (var messageJson in messagesJson)
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
                var attachmentsJson = messageJson["attachments"];
                var attachments = new List<Attachment>();
                foreach (var attachmentJson in attachmentsJson)
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

        public async Task<IEnumerable<Message>> GetMessagesAsync(string token, string channelId)
        {
            var result = new List<Message>();

            // We are going backwards from last message to first
            // ...collecting everything between them in batches
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
    }
}