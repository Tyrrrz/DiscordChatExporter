using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Runtime;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService : IExportService
    {
        private const string ResourceRootNamespace = "DiscordChatExporter.Core.Resources.ExportService";

        private readonly ISettingsService _settingsService;

        private readonly Template _plainTextTemplate;
        private readonly Template _htmlTemplate;
        private readonly Template _csvTemplate;

        private readonly string _htmlDarkCss;
        private readonly string _htmlLightCss;

        public ExportService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            // Templates
            _plainTextTemplate = Template.Parse(Assembly.GetExecutingAssembly()
                .GetManifestResourceString($"{ResourceRootNamespace}.PlainText.Template.txt"));
            _htmlTemplate = Template.Parse(Assembly.GetExecutingAssembly()
                .GetManifestResourceString($"{ResourceRootNamespace}.Html.Template.html"));
            //_csvTemplate = Template.Parse(Assembly.GetExecutingAssembly()
            //    .GetManifestResourceString($"{ResourceRootNamespace}.Csv.Template.csv"));

            // HTML styles
            var sharedCss = Assembly.GetExecutingAssembly()
                .GetManifestResourceString($"{ResourceRootNamespace}.Html.Shared.css");

            _htmlDarkCss = Assembly.GetExecutingAssembly()
                .GetManifestResourceString($"{ResourceRootNamespace}.Html.DarkTheme.css");
            _htmlDarkCss = sharedCss + Environment.NewLine + _htmlDarkCss;

            _htmlLightCss = Assembly.GetExecutingAssembly()
                .GetManifestResourceString($"{ResourceRootNamespace}.Html.LightTheme.css");
            _htmlLightCss = sharedCss + Environment.NewLine + _htmlLightCss;
        }

        public void Export(ExportFormat format, string filePath, ChannelChatLog log)
        {
            using (var output = File.CreateText(filePath))
            {
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
                if (format == ExportFormat.PlainText)
                {
                    _plainTextTemplate.Render(context);
                }

                else if (format == ExportFormat.HtmlDark)
                {
                    context.Evaluate(_plainTextTemplate.Page);
                }

                else if (format == ExportFormat.HtmlLight)
                {
                    context.Evaluate(_plainTextTemplate.Page);
                }

                else if (format == ExportFormat.Csv)
                {
                    context.Evaluate(_plainTextTemplate.Page);
                }

                else throw new ArgumentOutOfRangeException(nameof(format));
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