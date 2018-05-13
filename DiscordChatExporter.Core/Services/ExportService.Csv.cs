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
            foreach (var mentionedUser in message.Mentions.Users)
                content = Regex.Replace(content, $"<@!?{mentionedUser.Id}>", $"@{mentionedUser}");

            // Channel mentions (<#id>)
            foreach (var mentionedChannel in message.Mentions.Channels)
                content = content.Replace($"<#{mentionedChannel.Id}>", $"#{mentionedChannel.Name}");

            // Role mentions (<@&id>)
            foreach (var mentionedRole in message.Mentions.Roles)
                content = content.Replace($"<@&{mentionedRole.Id}>", $"@{mentionedRole.Name}");

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

            return content;
        }

        private async Task ExportAsCsvAsync(ChannelChatLog log, TextWriter output)
        {
            using (var writer = new CsvWriter(output))
            {
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
                        writer.WriteField(msg.Author.FullName);

                        // Date
                        var timeStampFormatted = msg.TimeStamp.ToString(_settingsService.DateFormat);
                        writer.WriteField(timeStampFormatted);

                        // Content
                        var contentFormatted = msg.Content.IsNotBlank() ? FormatMessageContentCsv(msg) : null;
                        writer.WriteField(contentFormatted);

                        // Attachments
                        var attachmentsFormatted = msg.Attachments.Select(a => a.Url).JoinToString(",");
                        writer.WriteField(attachmentsFormatted);

                        await writer.NextRecordAsync();
                    }
                }
            }
        }
    }
}
