using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.Html;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Domain.Internal.Extensions;
using Scriban;
using Scriban.Runtime;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal partial class HtmlMessageWriter : MessageWriter
    {
        private readonly TextWriter _writer;
        private readonly string _themeName;

        private readonly List<Message> _messageGroupBuffer = new List<Message>();

        private readonly Template _preambleTemplate;
        private readonly Template _messageGroupTemplate;
        private readonly Template _postambleTemplate;

        private long _messageCount;

        public HtmlMessageWriter(Stream stream, ExportContext context, UrlProcessor urlProcessor, string themeName)
            : base(stream, context, urlProcessor)
        {
            _writer = new StreamWriter(stream);
            _themeName = themeName;

            _preambleTemplate = Template.Parse(GetPreambleTemplateCode());
            _messageGroupTemplate = Template.Parse(GetMessageGroupTemplateCode());
            _postambleTemplate = Template.Parse(GetPostambleTemplateCode());
        }

        private TemplateContext CreateTemplateContext(IReadOnlyDictionary<string, object>? constants = null)
        {
            // Template context
            var templateContext = new TemplateContext
            {
                MemberRenamer = m => m.Name,
                MemberFilter = m => true,
                LoopLimit = int.MaxValue,
                StrictVariables = true
            };

            // Model
            var scriptObject = new ScriptObject();

            // Constants
            scriptObject.SetValue("Context", Context, true);
            scriptObject.SetValue("CoreStyleSheet", GetCoreStyleSheetCode(), true);
            scriptObject.SetValue("ThemeStyleSheet", GetThemeStyleSheetCode(_themeName), true);
            scriptObject.SetValue("HighlightJsStyleName", $"solarized-{_themeName.ToLowerInvariant()}", true);

            // Additional constants
            if (constants != null)
            {
                foreach (var (member, value) in constants)
                    scriptObject.SetValue(member, value, true);
            }

            // Functions
            scriptObject.Import("FormatDate",
                new Func<DateTimeOffset, string>(d => d.ToLocalString(Context.Request.DateFormat)));

            scriptObject.Import("FormatColorRgb",
                new Func<Color?, string?>(c => c != null ? $"rgb({c?.R}, {c?.G}, {c?.B})" : null));

            scriptObject.Import("TryGetUserColor",
                new Func<User, Color?>(Context.TryGetUserColor));

            scriptObject.Import("TryGetUserNick",
                new Func<User, string?>(u => Context.TryGetUserMember(u)?.Nick));

            scriptObject.Import("FormatMarkdown",
                new Func<string?, string>(m => FormatMarkdown(m)));

            scriptObject.Import("FormatEmbedMarkdown",
                new Func<string?, string>(m => FormatMarkdown(m, false)));

            // HACK: Scriban doesn't support async, so we have to resort to this and be careful about deadlocks.
            // TODO: move to Razor.
            scriptObject.Import("ResolveUrl",
                new Func<string?, string?>(u => ResolveUrlAsync(u).GetAwaiter().GetResult()));

            // Push model
            templateContext.PushGlobal(scriptObject);

            // Push output
            templateContext.PushOutput(new TextWriterOutput(_writer));

            return templateContext;
        }

        private string FormatMarkdown(string? markdown, bool isJumboAllowed = true) =>
            HtmlMarkdownVisitor.Format(Context, markdown ?? "", isJumboAllowed);

        private async Task WriteCurrentMessageGroupAsync()
        {
            var templateContext = CreateTemplateContext(new Dictionary<string, object>
            {
                ["MessageGroup"] = MessageGroup.Join(_messageGroupBuffer)
            });

            await templateContext.EvaluateAsync(_messageGroupTemplate.Page);
        }

        public override async Task WritePreambleAsync()
        {
            var templateContext = CreateTemplateContext();
            await templateContext.EvaluateAsync(_preambleTemplate.Page);
        }

        public override async Task WriteMessageAsync(Message message)
        {
            // If message group is empty or the given message can be grouped, buffer the given message
            if (!_messageGroupBuffer.Any() || MessageGroup.CanJoin(_messageGroupBuffer.Last(), message))
            {
                _messageGroupBuffer.Add(message);
            }
            // Otherwise, flush the group and render messages
            else
            {
                await WriteCurrentMessageGroupAsync();

                _messageGroupBuffer.Clear();
                _messageGroupBuffer.Add(message);
            }

            // Increment message count
            _messageCount++;
        }

        public override async Task WritePostambleAsync()
        {
            // Flush current message group
            if (_messageGroupBuffer.Any())
                await WriteCurrentMessageGroupAsync();

            var templateContext = CreateTemplateContext(new Dictionary<string, object>
            {
                ["MessageCount"] = _messageCount
            });

            await templateContext.EvaluateAsync(_postambleTemplate.Page);
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }

    internal partial class HtmlMessageWriter
    {
        private static readonly Assembly ResourcesAssembly = typeof(HtmlMessageWriter).Assembly;
        private static readonly string ResourcesNamespace = $"{ResourcesAssembly.GetName().Name}.Exporting.Writers.Html";

        private static string GetCoreStyleSheetCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlCore.css");

        private static string GetThemeStyleSheetCode(string themeName) =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.Html{themeName}.css");

        private static string GetPreambleTemplateCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlLayoutTemplate.html")
                .SubstringUntil("{{~ %SPLIT% ~}}");

        private static string GetMessageGroupTemplateCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlMessageGroupTemplate.html");

        private static string GetPostambleTemplateCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlLayoutTemplate.html")
                .SubstringAfter("{{~ %SPLIT% ~}}");
    }
}