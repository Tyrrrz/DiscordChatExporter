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

        public CsvMessageWriter(Stream stream, ExportContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        private string FormatMarkdown(string? markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

        public override async ValueTask WritePreambleAsync() =>
            await _writer.WriteLineAsync("AuthorID,Author,Date,Content,Attachments,Reactions");

        private async ValueTask WriteAttachmentsAsync(IReadOnlyList<Attachment> attachments)
        {
            var buffer = new StringBuilder();

            foreach (var attachment in attachments)
            {
                buffer
                    .AppendIfNotEmpty(',')
                    .Append(await Context.ResolveMediaUrlAsync(attachment.Url));
            }

            await _writer.WriteAsync(CsvEncode(buffer.ToString()));
        }

        private async ValueTask WriteReactionsAsync(IReadOnlyList<Reaction> reactions)
        {
            var buffer = new StringBuilder();

            foreach (var reaction in reactions)
            {
                buffer
                    .AppendIfNotEmpty(',')
                    .Append(reaction.Emoji.Name)
                    .Append(' ')
                    .Append('(')
                    .Append(reaction.Count)
                    .Append(')');
            }

            await _writer.WriteAsync(CsvEncode(buffer.ToString()));
        }

        public override async ValueTask WriteMessageAsync(Message message)
        {
            // Author ID
            await _writer.WriteAsync(CsvEncode(message.Author.Id));
            await _writer.WriteAsync(',');

            // Author name
            await _writer.WriteAsync(CsvEncode(message.Author.FullName));
            await _writer.WriteAsync(',');

            // Message timestamp
            await _writer.WriteAsync(CsvEncode(Context.FormatDate(message.Timestamp)));
            await _writer.WriteAsync(',');

            // Message content
            await _writer.WriteAsync(CsvEncode(FormatMarkdown(message.Content)));
            await _writer.WriteAsync(',');

            // Attachments
            await WriteAttachmentsAsync(message.Attachments);
            await _writer.WriteAsync(',');

            // Reactions
            await WriteReactionsAsync(message.Reactions);

            // Finish row
            await _writer.WriteLineAsync();
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