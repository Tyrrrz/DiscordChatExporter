using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Writers.Html;

namespace DiscordChatExporter.Core.Exporting.Writers
{
    internal class HtmlMessageWriter : MessageWriter
    {
        private readonly TextWriter _writer;
        private readonly string _themeName;

        private readonly List<Message> _messageGroupBuffer = new();

        private long _messageCount;

        public HtmlMessageWriter(Stream stream, ExportContext context, string themeName)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
            _themeName = themeName;
        }

        public override async ValueTask WritePreambleAsync()
        {
            var templateContext = new LayoutTemplateContext(Context, _themeName, _messageCount);

            await _writer.WriteLineAsync(
                await PreambleTemplate.RenderAsync(templateContext)
            );
        }

        private async ValueTask WriteMessageGroupAsync(MessageGroup messageGroup)
        {
            var templateContext = new MessageGroupTemplateContext(Context, messageGroup);

            await _writer.WriteLineAsync(
                await MessageGroupTemplate.RenderAsync(templateContext)
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
                await PostambleTemplate.RenderAsync(templateContext)
            );
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}