using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private string FormatMessageContentCsv(Message message)
        {
            var content = message.Content;

            // New lines
            content = content.Replace("\n", ", ");

            // User mentions (<@id> and <@!id>)
            foreach (var mentionedUser in message.MentionedUsers)
                content = Regex.Replace(content, $"<@!?{mentionedUser.Id}>", $"@{mentionedUser}");

            // Role mentions (<@&id>)
            foreach (var mentionedRole in message.MentionedRoles)
                content = content.Replace($"<@&{mentionedRole.Id}>", $"@{mentionedRole.Name}");

            // Channel mentions (<#id>)
            foreach (var mentionedChannel in message.MentionedChannels)
                content = content.Replace($"<#{mentionedChannel.Id}>", $"#{mentionedChannel.Name}");

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

            return content;
        }

        private async Task ExportAsCsvAsync(ChannelChatLog log, TextWriter output)
        {
            using (var writer = new CsvWriter(output))
            {
                // Generation info
                writer.WriteComment("https://github.com/Tyrrrz/DiscordChatExporter");

                // Guild and channel info
                writer.WriteComment($"Guild: {log.Guild.Name}");
                writer.WriteComment($"Channel: {log.Channel.Name}");
                writer.WriteComment($"Topic: {log.Channel.Topic}");
                writer.WriteComment($"Messages: {log.TotalMessageCount:N0}");

                // Headers
                writer.WriteField("Author");
                writer.WriteField("Date");
                writer.WriteField("Content");
                writer.WriteField("Attachments");
                await writer.NextRecordAsync();

                // Chat log
                foreach (var group in log.MessageGroups)
                {
                    // Messages
                    foreach (var msg in group.Messages)
                    {
                        // Author
                        writer.WriteField(msg.Author.FullName, true);

                        // Date
                        var timeStampFormatted = msg.TimeStamp.ToString(_settingsService.DateFormat);
                        writer.WriteField(timeStampFormatted);

                        // Content
                        var contentFormatted = msg.Content.IsNotBlank() ? FormatMessageContentCsv(msg) : null;
                        writer.WriteField(contentFormatted, true);

                        // Attachments
                        var attachmentsFormatted = msg.Attachments.Select(a => a.Url).JoinToString(",");
                        writer.WriteField(attachmentsFormatted, true);

                        await writer.NextRecordAsync();
                    }
                }
            }
        }
    }
}
