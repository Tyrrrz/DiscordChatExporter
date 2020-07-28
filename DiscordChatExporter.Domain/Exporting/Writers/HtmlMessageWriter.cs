using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.Html;
using RazorLight;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal class HtmlMessageWriter : MessageWriter
    {
        private readonly TextWriter _writer;
        private readonly string _themeName;

        private readonly RazorLightEngine _templateEngine;
        private readonly List<Message> _messageGroupBuffer = new List<Message>();

        private long _messageCount;

        public HtmlMessageWriter(Stream stream, ExportContext context, string themeName)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
            _themeName = themeName;

            _templateEngine = new RazorLightEngineBuilder()
                .EnableEncoding()
                .UseEmbeddedResourcesProject(typeof(HtmlMessageWriter).Assembly, $"{typeof(HtmlMessageWriter).Namespace}.Html")
                .Build();
        }

        public override async ValueTask WritePreambleAsync()
        {
            var templateContext = new LayoutTemplateContext(Context, _themeName, _messageCount);

            await _writer.WriteLineAsync(
                await _templateEngine.CompileRenderAsync("LayoutTemplate-Begin.cshtml", templateContext)
            );
        }

        private async ValueTask WriteMessageGroupAsync(MessageGroup messageGroup)
        {
            var templateContext = new MessageGroupTemplateContext(Context, messageGroup);

            await _writer.WriteLineAsync(
                await _templateEngine.CompileRenderAsync("MessageGroupTemplate.cshtml", templateContext)
            );
        }

        public override async ValueTask WriteMessageAsync(Message message)
        {
            // If message group is empty or the given message can be grouped, buffer the given message
            if (!_messageGroupBuffer.Any() || MessageGroup.CanJoin(_messageGroupBuffer.Last(), message))
            {
                _messageGroupBuffer.Add(message);
            }
            // Otherwise, flush the group and render messages
            else
            {
                await WriteMessageGroupAsync(MessageGroup.Join(_messageGroupBuffer));

                _messageGroupBuffer.Clear();
                _messageGroupBuffer.Add(message);
            }

            // Increment message count
            _messageCount++;
        }

        public override async ValueTask WritePostambleAsync()
        {
            // Flush current message group
            if (_messageGroupBuffer.Any())
                await WriteMessageGroupAsync(MessageGroup.Join(_messageGroupBuffer));

            var templateContext = new LayoutTemplateContext(Context, _themeName, _messageCount);

            await _writer.WriteLineAsync(
                await _templateEngine.CompileRenderAsync("LayoutTemplate-End.cshtml", templateContext)
            );
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}