using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exceptions;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    public partial class Exporter
    {
        private readonly DiscordClient _discord;

        public Exporter(DiscordClient discord) => _discord = discord;

        public async Task ExportChatLogAsync(Guild guild, Channel channel,
            string outputPath, ExportFormat format, string dateFormat, int? partitionLimit,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double>? progress = null)
        {
            // Get base file path from output path
            var baseFilePath = GetFilePathFromOutputPath(outputPath, format, guild, channel, after, before);

            // Create options
            var options = new RenderOptions(baseFilePath, format, partitionLimit);

            // Create context
            var mentionableUsers = new HashSet<User>(IdBasedEqualityComparer.Instance);
            var mentionableChannels = await _discord.GetGuildChannelsAsync(guild.Id);
            var mentionableRoles = guild.Roles;

            var context = new RenderContext(
                guild, channel, after, before, dateFormat,
                mentionableUsers, mentionableChannels, mentionableRoles
            );

            // Create renderer
            await using var renderer = new MessageRenderer(options, context);

            // Render messages
            var renderedAnything = false;
            await foreach (var message in _discord.GetMessagesAsync(channel.Id, after, before, progress))
            {
                // Add encountered users to the list of mentionable users
                var encounteredUsers = new List<User>();
                encounteredUsers.Add(message.Author);
                encounteredUsers.AddRange(message.MentionedUsers);

                mentionableUsers.AddRange(encounteredUsers);

                foreach (User u in encounteredUsers)
                {
                    if(!guild.Members.ContainsKey(u.Id))
                    {
                        var member = await _discord.GetGuildMemberAsync(guild.Id, u.Id);
                        guild.Members[u.Id] = member;
                    }
                }


                // Render message
                await renderer.RenderMessageAsync(message);
                renderedAnything = true;
            }

            // Throw if no messages were rendered
            if (!renderedAnything)
                throw DiscordChatExporterException.ChannelEmpty(channel);
        }
    }

    public partial class Exporter
    {
        public static string GetDefaultExportFileName(ExportFormat format,
            Guild guild, Channel channel,
            DateTimeOffset? after = null, DateTimeOffset? before = null)
        {
            var buffer = new StringBuilder();

            // Append guild and channel names
            buffer.Append($"{guild.Name} - {channel.Name} [{channel.Id}]");

            // Append date range
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

            // Append extension
            buffer.Append($".{format.GetFileExtension()}");

            // Replace invalid chars
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                buffer.Replace(invalidChar, '_');

            return buffer.ToString();
        }

        private static string GetFilePathFromOutputPath(string outputPath, ExportFormat format, Guild guild, Channel channel,
            DateTimeOffset? after, DateTimeOffset? before)
        {
            // Output is a directory
            if (Directory.Exists(outputPath) || string.IsNullOrWhiteSpace(Path.GetExtension(outputPath)))
            {
                var fileName = GetDefaultExportFileName(format, guild, channel, after, before);
                return Path.Combine(outputPath, fileName);
            }

            // Output is a file
            return outputPath;
        }
    }
}