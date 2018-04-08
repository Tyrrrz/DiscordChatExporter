using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private string FormatMessageContentPlainText(Message message)
        {
            var content = message.Content;

            // New lines
            content = content.Replace("\n", Environment.NewLine);

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

        private async Task ExportAsPlainTextAsync(ChannelChatLog log, TextWriter output)
        {
            // Generation info
            await output.WriteLineAsync("https://github.com/Tyrrrz/DiscordChatExporter");
            await output.WriteLineAsync();

            // Guild and channel info
            await output.WriteLineAsync('='.Repeat(48));
            await output.WriteLineAsync($"Guild: {log.Guild.Name}");
            await output.WriteLineAsync($"Channel: {log.Channel.Name}");
            await output.WriteLineAsync($"Topic: {log.Channel.Topic}");
            await output.WriteLineAsync($"Messages: {log.TotalMessageCount:N0}");
            await output.WriteLineAsync('='.Repeat(48));
            await output.WriteLineAsync();

            // Chat log
            foreach (var group in log.MessageGroups)
            {
                var timeStampFormatted = group.TimeStamp.ToString(_settingsService.DateFormat);
                await output.WriteLineAsync($"{group.Author.FullName}  [{timeStampFormatted}]");

                // Messages
                foreach (var message in group.Messages)
                {
                    // Content
                    if (message.Content.IsNotBlank())
                    {
                        var contentFormatted = FormatMessageContentPlainText(message);
                        await output.WriteLineAsync(contentFormatted);
                    }

                    // Attachments
                    foreach (var attachment in message.Attachments)
                    {
                        await output.WriteLineAsync(attachment.Url);
                    }
                }

                await output.WriteLineAsync();
            }
        }
    }
}