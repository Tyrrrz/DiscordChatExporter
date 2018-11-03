using System.Collections.Generic;
using System.IO;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Runtime;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService : IExportService
    {
        private readonly ISettingsService _settingsService;

        public ExportService(ISettingsService settingsService)
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
            var templateModel = new TemplateModel(format, chatLog,
                _settingsService.DateFormat, _settingsService.MessageGroupLimit);

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

                // Render template
                template.Render(context);
            }
        }

        private void ExportChatLogPartitions(IReadOnlyList<ChatLog> partitions, string filePath, ExportFormat format)
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
                var partitionFilePath = $"{fileNameWithoutExt}-{partitionNumber}{fileExt}";

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
            int? maxMessageCountPerPartition = null)
        {
            // If partitioning is disabled or there are fewer messages in chat log than the limit - process it without partitioning
            if (maxMessageCountPerPartition == null || chatLog.Messages.Count <= maxMessageCountPerPartition)
            {
                ExportChatLogSingle(chatLog, filePath, format);
            }
            // Otherwise split into partitions and export separately
            else
            {
                var partitions = chatLog.SplitIntoPartitions(maxMessageCountPerPartition.Value);
                ExportChatLogPartitions(partitions, filePath, format);
            }
        }
    }
}