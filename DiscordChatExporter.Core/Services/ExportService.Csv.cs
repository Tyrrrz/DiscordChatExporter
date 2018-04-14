using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            content = content.Replace("\n", "\\n");

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
            using (var writer = new CsvHelper.CsvWriter(output, false))
            {
                // Headers

                writer.WriteField("Name");
                writer.WriteField("Discriminator");
                writer.WriteField("Date");
                writer.WriteField("Content");
                writer.WriteField("Attachments");
                await writer.NextRecordAsync();

                // Chat
                foreach (var group in log.MessageGroups)
                {
                    foreach (var msg in group.Messages)
                    {
                        writer.WriteField(msg.Author.Name, true);
                        writer.WriteField(msg.Author.Discriminator.ToString().PadLeft(4, '0'));

                        var timeStampFormatted = msg.TimeStamp.ToString(_settingsService.DateFormat);
                        writer.WriteField(timeStampFormatted);

                        // Content
                        writer.WriteField(string.IsNullOrWhiteSpace(msg.Content) ? null : FormatMessageContentCsv(msg), true);

                        // Attachments
                        writer.WriteField(msg.Attachments.Select(x => x.Url).JoinToString(","), true);

                        await writer.NextRecordAsync();
                    }
                }
            }
        }
    }
}
