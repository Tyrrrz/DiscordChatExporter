using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Exceptions;
using DiscordChatExporter.Domain.Utilities;

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
            var contextMembers = new HashSet<Member>(IdBasedEqualityComparer.Instance);
            var contextChannels = await _discord.GetGuildChannelsAsync(guild.Id);
            var contextRoles = await _discord.GetGuildRolesAsync(guild.Id);

            var context = new ExportContext(
                guild, channel, after, before, dateFormat,
                contextMembers, contextChannels, contextRoles
            );

            await using var messageExporter = new MessageExporter(options, context);

            var exportedAnything = false;
            var encounteredUsers = new HashSet<User>(IdBasedEqualityComparer.Instance);
            await foreach (var message in _discord.GetMessagesAsync(channel.Id, after, before, progress))
            {
                // Resolve members for referenced users
                foreach (var referencedUser in message.MentionedUsers.Prepend(message.Author))
                {
                    if (encounteredUsers.Add(referencedUser))
                    {
                        var member =
                            await _discord.TryGetGuildMemberAsync(guild.Id, referencedUser) ??
                            Member.CreateForUser(referencedUser);

                        contextMembers.Add(member);
                    }
                }

                // Export message
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