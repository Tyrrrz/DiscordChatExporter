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

        public void Export(ExportFormat format, string filePath, ChatLog log)
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
            var templateModel = new TemplateModel(format, log, _settingsService.DateFormat);
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
    }
}