using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Runtime;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private readonly SettingsService _settingsService;

        public ExportService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private void ExportChatLogSingle(ChatLog chatLog, string filePath, ExportFormat format)
        {
            // Create template loader
            var loader = new TemplateLoader();

            // Get template
            var templateCode = loader.Load(format);
            var template = Template.Parse(templateCode);

            // Create template context
            var context = new TemplateContext
            {
                TemplateLoader = loader,
                MemberRenamer = m => m.Name,
                MemberFilter = m => true,
                LoopLimit = int.MaxValue,
                StrictVariables = true
            };

            // Create template model
            var templateModel = new TemplateModel(format, chatLog, _settingsService.DateFormat);

            context.PushGlobal(templateModel.GetScriptObject());

            // Create directory
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath.IsNotBlank())
                Directory.CreateDirectory(dirPath);

            // Render output
            using (var output = File.CreateText(filePath))
            {
                // Configure output
                context.PushOutput(new TextWriterOutput(output));

                // Render output
                context.Evaluate(template.Page);
            }
        }

        private void ExportChatLogPartitioned(IReadOnlyList<ChatLog> partitions, string filePath, ExportFormat format)
        {
            // Split file path into components
            var dirPath = Path.GetDirectoryName(filePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var fileExt = Path.GetExtension(filePath);

            // Export each partition separately
            var partitionNumber = 1;
            foreach (var partition in partitions)
            {
                // Compose new file name
                var partitionFilePath = $"{fileNameWithoutExt} [{partitionNumber} of {partitions.Count}]{fileExt}";

                // Compose full file path
                if (dirPath.IsNotBlank())
                    partitionFilePath = Path.Combine(dirPath, partitionFilePath);

                // Export
                ExportChatLogSingle(partition, partitionFilePath, format);

                // Increment partition number
                partitionNumber++;
            }
        }

        public void ExportChatLog(ChatLog chatLog, string filePath, ExportFormat format,
            int? partitionLimit = null)
        {
            // If partitioning is disabled or there are fewer messages in chat log than the limit - process it without partitioning
            if (partitionLimit == null || partitionLimit <= 0 || chatLog.Messages.Count <= partitionLimit)
            {
                ExportChatLogSingle(chatLog, filePath, format);
            }
            // Otherwise split into partitions and export separately
            else
            {
                // Create partitions by grouping up to X adjacent messages into separate chat logs
                var partitions = chatLog.Messages.GroupAdjacentWhile(g => g.Count < partitionLimit.Value)
                    .Select(g => new ChatLog(chatLog.Guild, chatLog.Channel, chatLog.From, chatLog.To, g, chatLog.Mentionables))
                    .ToArray();

                ExportChatLogPartitioned(partitions, filePath, format);
            }
        }
    }
}