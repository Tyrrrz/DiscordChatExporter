using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService : IExportService
    {
        private readonly ISettingsService _settingsService;

        public ExportService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private Template GetTemplate(ExportFormat format)
        {
            // Resource root namespace for all templates
            const string resourceRootNamespace = "DiscordChatExporter.Core.Resources.ExportTemplates";

            if (format == ExportFormat.PlainText)
            {
                // Get template code
                var raw = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.PlainText.Template.txt");

                // Parse
                return Template.Parse(raw);
            }

            if (format == ExportFormat.HtmlDark)
            {
                // Get css
                var sharedCss = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Html.Shared.css");
                var themeCss = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Html.DarkTheme.css");
                var css = sharedCss + Environment.NewLine + themeCss;

                // Get template code
                var raw = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Html.Template.html");
                
                // Inject css
                raw = raw.Replace("<style></style>", $"<style>{css}</style>");

                // Parse
                return Template.Parse(raw);
            }

            if (format == ExportFormat.HtmlLight)
            {
                // Get css
                var sharedCss = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Html.Shared.css");
                var themeCss = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Html.LightTheme.css");
                var css = sharedCss + Environment.NewLine + themeCss;

                // Get template code
                var raw = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Html.Template.html");

                // Inject css
                raw = raw.Replace("<style></style>", $"<style>{css}</style>");

                // Parse
                return Template.Parse(raw);
            }

            if (format == ExportFormat.Csv)
            {
                // Get template code
                var raw = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Csv.Template.csv");

                // Parse
                return Template.Parse(raw);
            }

            throw new ArgumentOutOfRangeException(nameof(format));
        }

        public void Export(ExportFormat format, string filePath, ChannelChatLog log)
        {
            // Get template
            var template = GetTemplate(format);

            // Create template context
            var context = new TemplateContext();
            context.MemberRenamer = m => m.Name;

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