using System.Reflection;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        public class TemplateLoader : ITemplateLoader
        {
            private const string ResourceRootNamespace = "DiscordChatExporter.Core.Resources.ExportTemplates";

            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return $"{ResourceRootNamespace}.{templateName}";
            }

            public string GetPath(ExportFormat format)
            {
                return $"{ResourceRootNamespace}.{format}.{format.GetFileExtension()}";
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