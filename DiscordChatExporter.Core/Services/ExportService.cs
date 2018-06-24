using System.IO;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Runtime;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService : IExportService
    {
        private static readonly MemberRenamerDelegate TemplateMemberRenamer = m => m.Name;
        private static readonly MemberFilterDelegate TemplateMemberFilter = m => true;

        private readonly ISettingsService _settingsService;

        public ExportService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void Export(ExportFormat format, string filePath, ChannelChatLog log)
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
                MemberRenamer = TemplateMemberRenamer,
                MemberFilter = TemplateMemberFilter
            };

            // Create template model
            var templateModel = new TemplateModel(format, log, _settingsService.DateFormat);
            context.PushGlobal(templateModel.GetScriptObject());

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