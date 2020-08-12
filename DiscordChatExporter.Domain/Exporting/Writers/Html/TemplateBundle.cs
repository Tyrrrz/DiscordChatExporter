using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MiniRazor;
using Tyrrrz.Extensions;

[assembly: InternalsVisibleTo(DiscordChatExporter.Domain.Exporting.Writers.Html.TemplateBundle.PreambleTemplateAssemblyName)]
[assembly: InternalsVisibleTo(DiscordChatExporter.Domain.Exporting.Writers.Html.TemplateBundle.MessageGroupTemplateAssemblyName)]
[assembly: InternalsVisibleTo(DiscordChatExporter.Domain.Exporting.Writers.Html.TemplateBundle.PostambleTemplateAssemblyName)]

namespace DiscordChatExporter.Domain.Exporting.Writers.Html
{
    internal partial class TemplateBundle
    {
        public const string PreambleTemplateAssemblyName = "RazorAssembly_Preamble";
        public const string MessageGroupTemplateAssemblyName = "RazorAssembly_MessageGroup";
        public const string PostambleTemplateAssemblyName = "RazorAssembly_Postamble";

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

            var engine = new MiniRazorTemplateEngine();

            var preambleTemplate = engine.Compile(preambleTemplateSource, PreambleTemplateAssemblyName, ns);
            var messageGroupTemplate = engine.Compile(messageGroupTemplateSource, MessageGroupTemplateAssemblyName, ns);
            var postambleTemplate = engine.Compile(postambleTemplateSource, PostambleTemplateAssemblyName, ns);

            return new TemplateBundle(preambleTemplate, messageGroupTemplate, postambleTemplate);
        });

        public static async ValueTask<TemplateBundle> ResolveAsync() =>
            _lastBundle ??= await CompileAsync();
    }
}