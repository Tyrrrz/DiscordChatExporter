using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering;
using DiscordChatExporter.Core.Services.Logic;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public class ExportService
    {
        private readonly SettingsService _settingsService;
        private readonly DataService _dataService;

        public ExportService(SettingsService settingsService, DataService dataService)
        {
            _settingsService = settingsService;
            _dataService = dataService;
        }

        private string GetFilePathFromOutputPath(string outputPath, ExportFormat format, RenderContext context)
        {
            // Output is a directory
            if (Directory.Exists(outputPath) || string.IsNullOrWhiteSpace(Path.GetExtension(outputPath)))
            {
                var fileName = ExportLogic.GetDefaultExportFileName(format, context.Guild, context.Channel, context.After, context.Before);
                return Path.Combine(outputPath, fileName);
            }

            // Output is a file
            return outputPath;
        }

        private IMessageRenderer CreateRenderer(string outputPath, int partitionIndex, ExportFormat format, RenderContext context)
        {
            var filePath = ExportLogic.GetExportPartitionFilePath(
                GetFilePathFromOutputPath(outputPath, format, context),
                partitionIndex);

            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            // Create renderer

            if (format == ExportFormat.PlainText)
                return new PlainTextMessageRenderer(filePath, context);

            if (format == ExportFormat.Csv)
                return new CsvMessageRenderer(filePath, context);

            if (format == ExportFormat.HtmlDark)
                return new HtmlMessageRenderer(filePath, context, "Dark");

            if (format == ExportFormat.HtmlLight)
                return new HtmlMessageRenderer(filePath, context, "Light");

            throw new InvalidOperationException($"Unknown export format [{format}].");
        }

        public async Task ExportChatLogAsync(AuthToken token, Guild guild, Channel channel,
            string outputPath, ExportFormat format, int? partitionLimit,
            DateTimeOffset? after = null, DateTimeOffset? before = null, IProgress<double>? progress = null)
        {
            // Create context
            var mentionableUsers = new HashSet<User>(IdBasedEqualityComparer.Instance);
            var mentionableChannels = await _dataService.GetGuildChannelsAsync(token, guild.Id);
            var mentionableRoles = await _dataService.GetGuildRolesAsync(token, guild.Id);

            var context = new RenderContext
            (
                guild, channel, after, before, _settingsService.DateFormat,
                mentionableUsers, mentionableChannels, mentionableRoles
            );

            // Render messages
            var partitionIndex = 0;
            var partitionMessageCount = 0;
            var renderer = CreateRenderer(outputPath, partitionIndex, format, context);

            await foreach (var message in _dataService.GetMessagesAsync(token, channel.Id, after, before, progress))
            {
                // Add encountered users to the list of mentionable users
                mentionableUsers.Add(message.Author);
                mentionableUsers.AddRange(message.MentionedUsers);

                // If new partition is required, reset renderer
                if (partitionLimit != null && partitionLimit > 0 && partitionMessageCount >= partitionLimit)
                {
                    partitionIndex++;
                    partitionMessageCount = 0;

                    // Flush old renderer and create a new one
                    await renderer.DisposeAsync();
                    renderer = CreateRenderer(outputPath, partitionIndex, format, context);
                }

                // Render message
                await renderer.RenderMessageAsync(message);
                partitionMessageCount++;
            }

            // Flush last renderer
            await renderer.DisposeAsync();
        }
    }
}