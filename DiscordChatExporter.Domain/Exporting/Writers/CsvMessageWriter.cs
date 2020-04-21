using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Domain.Internal;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal class CsvMessageWriter : MessageWriterBase
    {
        private readonly TextWriter _writer;

        public CsvMessageWriter(Stream stream, RenderContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        private string EncodeValue(string value)
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        private string FormatMarkdown(string markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown);

        private string FormatMessage(Message message)
        {
            var buffer = new StringBuilder();

            buffer
                .Append(EncodeValue(message.Author.Id)).Append(',')
                .Append(EncodeValue(message.Author.FullName)).Append(',')
                .Append(EncodeValue(message.Timestamp.ToLocalString(Context.DateFormat))).Append(',')
                .Append(EncodeValue(FormatMarkdown(message.Content))).Append(',')
                .Append(EncodeValue(message.Attachments.Select(a => a.Url).JoinToString(","))).Append(',')
                .Append(EncodeValue(message.Reactions.Select(r => $"{r.Emoji.Name} ({r.Count})").JoinToString(",")));

            return buffer.ToString();
        }

        public override async Task WritePreambleAsync() =>
            await _writer.WriteLineAsync("AuthorID,Author,Date,Content,Attachments,Reactions");

        public override async Task WriteMessageAsync(Message message) =>
            await _writer.WriteLineAsync(FormatMessage(message));

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}