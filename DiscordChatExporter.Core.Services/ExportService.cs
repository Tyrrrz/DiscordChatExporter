using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public class ExportService
    {
        private readonly SettingsService _settingsService;

        public ExportService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private IChatLogRenderer CreateRenderer(ChatLog chatLog, ExportFormat format)
        {
            if (format == ExportFormat.PlainText)
                return new PlainTextChatLogRenderer(chatLog, _settingsService.DateFormat);

            if (format == ExportFormat.HtmlDark)
                return new HtmlChatLogRenderer(chatLog, "Dark", _settingsService.DateFormat);

            if (format == ExportFormat.HtmlLight)
                return new HtmlChatLogRenderer(chatLog, "Light", _settingsService.DateFormat);

            if (format == ExportFormat.Csv)
                return new CsvChatLogRenderer(chatLog, _settingsService.DateFormat);

            throw new ArgumentOutOfRangeException(nameof(format), $"Unknown format [{format}].");
        }

        private async Task ExportChatLogAsync(ChatLog chatLog, string filePath, ExportFormat format)
        {
            // Create output directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null)
                Directory.CreateDirectory(dirPath);

            // Render chat log to output file
            using (var writer = File.CreateText(filePath))
                await CreateRenderer(chatLog, format).RenderAsync(writer);
        }

        public async Task ExportChatLogAsync(ChatLog chatLog, string filePath, ExportFormat format,
            int? partitionLimit = null)
        {
            // If partitioning is disabled or there are fewer messages in chat log than the limit - process it without partitioning
            if (partitionLimit == null || partitionLimit <= 0 || chatLog.Messages.Count <= partitionLimit)
            {
                await ExportChatLogAsync(chatLog, filePath, format);
            }
            // Otherwise split into partitions and export separately
            else
            {
                // Create partitions by grouping up to X contiguous messages into separate chat logs
                var partitions = chatLog.Messages.GroupContiguous(g => g.Count < partitionLimit.Value)
                    .Select(g => new ChatLog(chatLog.Guild, chatLog.Channel, chatLog.From, chatLog.To, g, chatLog.Mentionables))
                    .ToArray();

                // Split file path into components
                var dirPath = Path.GetDirectoryName(filePath);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                var fileExt = Path.GetExtension(filePath);

                // Export each partition separately
                var partitionNumber = 1;
                foreach (var partition in partitions)
                {
                    // Compose new file name
                    var partitionFilePath = $"{fileNameWithoutExt} [{partitionNumber} of {partitions.Length}]{fileExt}";

                    // Compose full file path
                    if (dirPath != null)
                        partitionFilePath = Path.Combine(dirPath, partitionFilePath);

                    // Export
                    await ExportChatLogAsync(partition, partitionFilePath, format);

                    // Increment partition number
                    partitionNumber++;
                }
            }
        }
    }
}