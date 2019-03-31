using Scriban.Parsing;
using Scriban.Runtime;
using Scriban;
using System.Reflection;
using System.Threading.Tasks;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Render
{
    public partial class HtmlChatLogRenderer
    {
        private class TemplateLoader : ITemplateLoader
        {
            private const string ResourceRootNamespace = "DiscordChatExporter.Core.Render.Resources";

            public string Load(string templatePath) =>
                Assembly.GetExecutingAssembly().GetManifestResourceString($"{ResourceRootNamespace}.{templatePath}");

            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) => templateName;

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath) => Load(templatePath);

            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
                new ValueTask<string>(Load(templatePath));
        }
    }
}