using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Exceptions;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    public partial class ChannelExporter
    {
        private readonly DiscordClient _discord;

        public ChannelExporter(DiscordClient discord) => _discord = discord;

        public ChannelExporter(AuthToken token) : this(new DiscordClient(token)) {}

        public async Task ExportAsync(
            Guild guild,
            Channel channel,
            string outputPath,
            ExportFormat format,
            string dateFormat,
            int? partitionLimit,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null,
            IProgress<double>? progress = null)
        {
            var baseFilePath = GetFilePathFromOutputPath(guild, channel, outputPath, format, after, before);

            // Options
            var options = new ExportOptions(baseFilePath, format, partitionLimit);

            // Context
            var mentionableUsers = new HashSet<User>(IdBasedEqualityComparer.Instance);
            var mentionableChannels = await _discord.GetGuildChannelsAsync(guild.Id);
            var mentionableRoles = guild.Roles;

            var context = new ExportContext(
                guild, channel, after, before, dateFormat,
                mentionableUsers, mentionableChannels, mentionableRoles
            );

            await using var messageExporter = new MessageExporter(options, context);

            var exportedAnything = false;
            await foreach (var message in _discord.GetMessagesAsync(channel.Id, after, before, progress))
            {
                // Add encountered users to the list of mentionable users
                var encounteredUsers = new List<User>();
                encounteredUsers.Add(message.Author);
                encounteredUsers.AddRange(message.MentionedUsers);

                mentionableUsers.AddRange(encounteredUsers);

                foreach (User u in encounteredUsers)
                {
                    if (!guild.Members.ContainsKey(u.Id))
                    {
                        var member = await _discord.GetGuildMemberAsync(guild.Id, u.Id);
                        guild.Members[u.Id] = member;
                    }
                }

                // Render message
                await messageExporter.ExportMessageAsync(message);
                exportedAnything = true;
            }

            // Throw if no messages were exported
            if (!exportedAnything)
                throw DiscordChatExporterException.ChannelEmpty(channel);
        }
    }

    public partial class ChannelExporter
    {
        public static string GetDefaultExportFileName(
            Guild guild,
            Channel channel,
            ExportFormat format,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {
            var buffer = new StringBuilder();

            // Guild and channel names
            buffer.Append($"{guild.Name} - {channel.Name} [{channel.Id}]");

            // Date range
            if (after != null || before != null)
            {
                buffer.Append(" (");

                // Both 'after' and 'before' are set
                if (after != null && before != null)
                {
                    buffer.Append($"{after:yyyy-MM-dd} to {before:yyyy-MM-dd}");
                }
                // Only 'after' is set
                else if (after != null)
                {
                    buffer.Append($"after {after:yyyy-MM-dd}");
                }
                // Only 'before' is set
                else
                {
                    buffer.Append($"before {before:yyyy-MM-dd}");
                }

                buffer.Append(")");
            }

            // File extension
            buffer.Append($".{format.GetFileExtension()}");

            // Replace invalid chars
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                buffer.Replace(invalidChar, '_');

            return buffer.ToString();
        }

        private static string GetFilePathFromOutputPath(
            Guild guild,
            Channel channel,
            string outputPath,
            ExportFormat format,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {
            // Output is a directory
            if (Directory.Exists(outputPath) || string.IsNullOrWhiteSpace(Path.GetExtension(outputPath)))
            {
                var fileName = GetDefaultExportFileName(guild, channel, format, after, before);
                return Path.Combine(outputPath, fileName);
            }

            // Output is a file
            return outputPath;
        }
    }
}