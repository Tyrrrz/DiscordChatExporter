using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal partial class CsvMessageWriter : MessageWriter
    {
        private readonly TextWriter _writer;

        public CsvMessageWriter(Stream stream, ExportContext context, UrlProcessor urlProcessor)
            : base(stream, context, urlProcessor)
        {
            _writer = new StreamWriter(stream);
        }

        private string FormatMarkdown(string? markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

        private async Task WriteRowAsync() =>
            await _writer.WriteLineAsync();

        private async Task WriteColumnAsync(string value) =>
            await _writer.WriteAsync(CsvEncode(value) + ',');

        public override async Task WritePreambleAsync() =>
            await _writer.WriteLineAsync("AuthorID,Author,Date,Content,Attachments,Reactions");

        public override async Task WriteMessageAsync(Message message)
        {
            // Author
            await WriteColumnAsync(message.Author.Id);
            await WriteColumnAsync(message.Author.FullName);

            // Message
            await WriteColumnAsync(message.Timestamp.ToLocalString(Context.Request.DateFormat));
            await WriteColumnAsync(FormatMarkdown(message.Content));

            // Attachments
            var attachmentsBuffer = new StringBuilder();
            foreach (var attachment in message.Attachments)
            {
                attachmentsBuffer
                    .AppendIfEmpty(',')
                    .Append(await ResolveUrlAsync(attachment.Url));
            }

            await WriteColumnAsync(attachmentsBuffer.ToString());

            // Reactions
            var reactionsBuffer = new StringBuilder();
            foreach (var reaction in message.Reactions)
            {
                attachmentsBuffer
                    .AppendIfEmpty(',')
                    .Append(reaction.Emoji.Name)
                    .Append(' ')
                    .Append('(')
                    .Append(reaction.Count)
                    .Append(')');
            }

            await WriteColumnAsync(reactionsBuffer.ToString());

            // Finish row
            await WriteRowAsync();
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