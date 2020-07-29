using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MiniRazor;
using Tyrrrz.Extensions;

[assembly: InternalsVisibleTo("RazorAssembly")]

namespace DiscordChatExporter.Domain.Exporting.Writers.Html
{
    internal partial class TemplateBundle
    {
        public MiniRazorTemplateDescriptor PreambleTemplate { get; }

        public MiniRazorTemplateDescriptor MessageGroupTemplate { get; }

        public MiniRazorTemplateDescriptor PostambleTemplate { get; }

        public TemplateBundle(
            MiniRazorTemplateDescriptor preambleTemplate,
            MiniRazorTemplateDescriptor messageGroupTemplate,
            MiniRazorTemplateDescriptor postambleTemplate)
        {
            PreambleTemplate = preambleTemplate;
            MessageGroupTemplate = messageGroupTemplate;
            PostambleTemplate = postambleTemplate;
        }
    }

    internal partial class TemplateBundle
    {
        private static TemplateBundle? _lastBundle;

        // This is very CPU-heavy
        private static async ValueTask<TemplateBundle> CompileAsync() => await Task.Run(() =>
        {
            var ns = typeof(TemplateBundle).Namespace!;

            var preambleTemplateSource = typeof(HtmlMessageWriter).Assembly
                .GetManifestResourceString($"{ns}.PreambleTemplate.cshtml");

            var messageGroupTemplateSource = typeof(HtmlMessageWriter).Assembly
                .GetManifestResourceString($"{ns}.MessageGroupTemplate.cshtml");

            var postambleTemplateSource = typeof(HtmlMessageWriter).Assembly
                .GetManifestResourceString($"{ns}.PostambleTemplate.cshtml");

            var engine = new MiniRazorTemplateEngine("RazorAssembly", ns);

            var preambleTemplate = engine.Compile(preambleTemplateSource);
            var messageGroupTemplate = engine.Compile(messageGroupTemplateSource);
            var postambleTemplate = engine.Compile(postambleTemplateSource);

            return new TemplateBundle(preambleTemplate, messageGroupTemplate, postambleTemplate);
        });

        public static async ValueTask<TemplateBundle> ResolveAsync() =>
            _lastBundle ??= await CompileAsync();
    }
}