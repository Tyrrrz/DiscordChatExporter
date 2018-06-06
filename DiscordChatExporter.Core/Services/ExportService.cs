﻿using System.IO;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Runtime;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService : IExportService
    {
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
            var context = new TemplateContext();
            context.MemberRenamer = m => m.Name;
            context.TemplateLoader = loader;

            // Create script object
            var scriptObject = new ScriptObject();

            // Import date format
            scriptObject.SetValue("DateFormat", _settingsService.DateFormat, true);

            // Import model
            scriptObject.Import(log, context.MemberFilter, context.MemberRenamer);

            // Import template functions
            scriptObject.Import(typeof(TemplateFunctions), context.MemberFilter, context.MemberRenamer);

            // Add script object
            context.PushGlobal(scriptObject);

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