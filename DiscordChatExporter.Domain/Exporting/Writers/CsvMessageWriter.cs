using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal partial class CsvMessageWriter : MessageWriter
    {
        private readonly TextWriter _writer;

        public CsvMessageWriter(Stream stream, ExportContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        private string FormatMarkdown(string? markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

        public override async Task WritePreambleAsync() =>
            await _writer.WriteLineAsync("AuthorID,Author,Date,Content,Attachments,Reactions");

        public override async Task WriteMessageAsync(Message message)
        {
            var buffer = new StringBuilder();

            buffer
                .Append(CsvEncode(message.Author.Id)).Append(',')
                .Append(CsvEncode(message.Author.FullName)).Append(',')
                .Append(CsvEncode(message.Timestamp.ToLocalString(Context.Request.DateFormat))).Append(',')
                .Append(CsvEncode(FormatMarkdown(message.Content))).Append(',')
                .Append(CsvEncode(message.Attachments.Select(a => a.Url).JoinToString(","))).Append(',')
                .Append(CsvEncode(message.Reactions.Select(r => $"{r.Emoji.Name} ({r.Count})").JoinToString(",")));

            await _writer.WriteLineAsync(buffer.ToString());
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }

    internal partial class CsvMessageWriter
    {
        private static string CsvEncode(string value)
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
    }
}