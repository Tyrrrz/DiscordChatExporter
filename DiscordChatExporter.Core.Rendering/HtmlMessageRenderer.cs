using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Rendering.Logic;
using Scriban;
using Scriban.Runtime;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Rendering
{
    public partial class HtmlMessageRenderer : MessageRendererBase
    {
        private readonly string _themeName;
        private readonly List<Message> _messageGroupBuffer = new List<Message>();

        private bool _isLeadingBlockRendered;

        public HtmlMessageRenderer(string filePath, RenderContext context, string themeName)
            : base(filePath, context)
        {
            _themeName = themeName;
        }

        private MessageGroup GetCurrentMessageGroup()
        {
            var firstMessage = _messageGroupBuffer.First();
            return new MessageGroup(firstMessage.Author, firstMessage.Timestamp, _messageGroupBuffer);
        }

        private async Task RenderLeadingBlockAsync()
        {
            var template = Template.Parse(GetLeadingBlockTemplateCode());
            var templateContext = CreateTemplateContext();
            var scriptObject = CreateScriptObject(Context, _themeName);

            templateContext.PushGlobal(scriptObject);
            templateContext.PushOutput(new TextWriterOutput(Writer));

            await templateContext.EvaluateAsync(template.Page);
        }

        private async Task RenderTrailingBlockAsync()
        {
            var template = Template.Parse(GetTrailingBlockTemplateCode());
            var templateContext = CreateTemplateContext();
            var scriptObject = CreateScriptObject(Context, _themeName);

            templateContext.PushGlobal(scriptObject);
            templateContext.PushOutput(new TextWriterOutput(Writer));

            await templateContext.EvaluateAsync(template.Page);
        }

        private async Task RenderCurrentMessageGroupAsync()
        {
            var template = Template.Parse(GetMessageGroupTemplateCode());
            var templateContext = CreateTemplateContext();
            var scriptObject = CreateScriptObject(Context, _themeName);

            scriptObject.SetValue("MessageGroup", GetCurrentMessageGroup(), true);

            templateContext.PushGlobal(scriptObject);
            templateContext.PushOutput(new TextWriterOutput(Writer));

            await templateContext.EvaluateAsync(template.Page);
        }

        public override async Task RenderMessageAsync(Message message)
        {
            // Render leading block if it's the first entry
            if (!_isLeadingBlockRendered)
            {
                await RenderLeadingBlockAsync();
                _isLeadingBlockRendered = true;
            }

            // If message group is empty or the given message can be grouped, buffer the given message
            if (!_messageGroupBuffer.Any() || HtmlRenderingLogic.CanBeGrouped(_messageGroupBuffer.Last(), message))
            {
                _messageGroupBuffer.Add(message);
            }
            // Otherwise, flush the group and render messages
            else
            {
                await RenderCurrentMessageGroupAsync();

                _messageGroupBuffer.Clear();
                _messageGroupBuffer.Add(message);
            }
        }

        public override async ValueTask DisposeAsync()
        {
            // Leading block (can happen if no message were rendered)
            if (!_isLeadingBlockRendered)
                await RenderLeadingBlockAsync();

            // Flush current message group
            if (_messageGroupBuffer.Any())
                await RenderCurrentMessageGroupAsync();

            // Trailing block
            await RenderTrailingBlockAsync();

            await base.DisposeAsync();
        }
    }

    public partial class HtmlMessageRenderer
    {
        private static readonly Assembly ResourcesAssembly = typeof(HtmlRenderingLogic).Assembly;
        private static readonly string ResourcesNamespace = $"{ResourcesAssembly.GetName().Name}.Resources";

        private static string GetCoreStyleSheetCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlCore.css");

        private static string GetThemeStyleSheetCode(string themeName) =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.Html{themeName}.css");

        private static string GetLeadingBlockTemplateCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlLayoutTemplate.html")
                .SubstringUntil("{{~ %SPLIT% ~}}");

        private static string GetTrailingBlockTemplateCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlLayoutTemplate.html")
                .SubstringAfter("{{~ %SPLIT% ~}}");

        private static string GetMessageGroupTemplateCode() =>
            ResourcesAssembly
                .GetManifestResourceString($"{ResourcesNamespace}.HtmlMessageGroupTemplate.html");

        private static ScriptObject CreateScriptObject(RenderContext context, string themeName)
        {
            var scriptObject = new ScriptObject();

            // Constants
            scriptObject.SetValue("Context", context, true);
            scriptObject.SetValue("CoreStyleSheet", GetCoreStyleSheetCode(), true);
            scriptObject.SetValue("ThemeStyleSheet", GetThemeStyleSheetCode(themeName), true);
            scriptObject.SetValue("HighlightJsStyleName", $"solarized-{themeName.ToLowerInvariant()}", true);

            // Functions

            scriptObject.Import("FormatDate",
                new Func<DateTimeOffset, string>(d => SharedRenderingLogic.FormatDate(d, context.DateFormat)));

            scriptObject.Import("FormatMarkdown",
                new Func<string, string>(m => HtmlRenderingLogic.FormatMarkdown(context, m)));

            return scriptObject;
        }

        private static TemplateContext CreateTemplateContext() =>
            new TemplateContext
            {
                MemberRenamer = m => m.Name,
                MemberFilter = m => true,
                LoopLimit = int.MaxValue,
                StrictVariables = true
            };
    }
}