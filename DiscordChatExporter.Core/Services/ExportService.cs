using System;
using System.IO;
using System.Reflection;
using DiscordChatExporter.Core.Internal;
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

        private Template GetTemplate(ExportFormat format)
        {
            // Resource root namespace for all templates
            const string resourceRootNamespace = "DiscordChatExporter.Core.Resources.ExportService";

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
                var raw = Assembly.GetExecutingAssembly()
                    .GetManifestResourceString($"{resourceRootNamespace}.Csv.Template.csv");
                return Template.Parse(raw);
            }

            throw new ArgumentOutOfRangeException(nameof(format));
        }

        public void Export(ExportFormat format, string filePath, ChannelChatLog log)
        {
            using (var output = File.CreateText(filePath))
            {
                // Get template
                var template = GetTemplate(format);

                // Create template context
                var context = new TemplateContext();
                context.MemberRenamer = m => m.Name;

                // Add output
                context.PushOutput(new TextWriterOutput(output));

                // Import model
                var scriptObject = new ScriptObject();
                scriptObject.Import(log, null, context.MemberRenamer);
                context.PushGlobal(scriptObject);

                // Render template
                template.Render(context);
            }
        }
    }

    public partial class ExportService
    {
        // TODO: use bytesize
        private static string FormatFileSize(long fileSize)
        {
            string[] units = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
            double size = fileSize;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }
    }
}