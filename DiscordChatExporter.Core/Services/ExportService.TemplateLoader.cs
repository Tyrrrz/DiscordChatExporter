using System.Reflection;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private class TemplateLoader : ITemplateLoader
        {
            private const string ResourceRootNamespace = "DiscordChatExporter.Core.Resources.ExportTemplates";

            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return $"{ResourceRootNamespace}.{templateName}";
            }

            public string GetPath(ExportFormat format)
            {
                return $"{ResourceRootNamespace}.{format}.Template.{format.GetFileExtension()}";
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return Assembly.GetExecutingAssembly().GetManifestResourceString(templatePath);
            }

            public string Load(ExportFormat format)
            {
                return Assembly.GetExecutingAssembly().GetManifestResourceString(GetPath(format));
            }
        }
    }
}