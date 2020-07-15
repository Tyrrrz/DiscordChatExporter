using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    internal partial class MessageExporter : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ExportContext _context;
        private readonly string _outputBaseFilePath;

        private readonly Dictionary<string, string> _mediaPathMap = new Dictionary<string, string>();

        private long _renderedMessageCount;
        private int _partitionIndex;
        private MessageWriter? _writer;

        public MessageExporter(HttpClient httpClient,ExportContext context)
        {
            _httpClient = httpClient;
            _context = context;

            _outputBaseFilePath = context.Request.GetOutputBaseFilePath();
        }

        public MessageExporter(ExportContext context)
            : this(Singleton.HttpClient, context)
        {
        }

        private bool IsPartitionLimitReached() =>
            _renderedMessageCount > 0 &&
            _context.Request.PartitionLimit != null &&
            _context.Request.PartitionLimit != 0 &&
            _renderedMessageCount % _context.Request.PartitionLimit == 0;

        private async Task ResetWriterAsync()
        {
            if (_writer != null)
            {
                await _writer.WritePostambleAsync();
                await _writer.DisposeAsync();
                _writer = null;
            }
        }

        private async Task<MessageWriter> GetWriterAsync()
        {
            // Ensure partition limit has not been exceeded
            if (IsPartitionLimitReached())
            {
                await ResetWriterAsync();
                _partitionIndex++;
            }

            // Writer is still valid - return
            if (_writer != null)
                return _writer;

            var filePath = GetPartitionFilePath(_outputBaseFilePath, _partitionIndex);

            var dirPath = Path.GetDirectoryName(_outputBaseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);

            var writer = CreateMessageWriter(filePath, _context.Request.Format, _context);
            await writer.WritePreambleAsync();

            return _writer = writer;
        }

        private async Task<string> RetargetMediaUrlAsync(string url)
        {
            if (_mediaPathMap.TryGetValue(url, out var cachedFilePath))
                return cachedFilePath;

            var outputDirPath = Path.Combine(_options.BaseDirPath, $"{Path.GetFileNameWithoutExtension(_outputBaseFilePath)}_Files");
            Directory.CreateDirectory(outputDirPath);

            var ext = Path.GetExtension(new Uri(url).AbsolutePath);
            var filePath = Path.Combine(outputDirPath, $"{Guid.NewGuid()}{ext}");

            await _httpClient.DownloadAsync(url, filePath);

            return _mediaPathMap[url] = filePath;
        }

        private async Task<Message> RetargetMediaAsync(Message message)
        {
            // Media that we're interested in downloading:
            // - Author avatar
            // - Image attachments
            // - Images in embeds

            var avatarUrl = await RetargetMediaUrlAsync(message.Author.AvatarUrl);
            var author = new User(
                message.Author.Id,
                message.Author.IsBot,
                message.Author.Discriminator,
                message.Author.Name,
                avatarUrl
            );

            var attachments = new List<Attachment>();
            foreach (var attachment in message.Attachments)
            {
                if (attachment.IsImage)
                {
                    var attachmentUrl = await RetargetMediaUrlAsync(attachment.Url);
                    attachments.Add(new Attachment(
                        attachment.Id,
                        attachmentUrl,
                        attachment.FileName,
                        attachment.Width,
                        attachment.Height,
                        attachment.FileSize
                    ));
                }
                else
                {
                    attachments.Add(attachment);
                }
            }

            var embeds = new List<Embed>();
            foreach (var embed in message.Embeds)
            {
                // TODO
                embeds.Add(embed);
            }

            return new Message(
                message.Id,
                message.Type,
                author,
                message.Timestamp,
                message.EditedTimestamp,
                message.IsPinned,
                message.Content,
                attachments,
                embeds,
                message.Reactions,
                message.MentionedUsers
            );
        }

        public async Task ExportMessageAsync(Message message)
        {
            var writer = await GetWriterAsync();

            if (_context.Request.RewriteMedia)
                message = await RetargetMediaAsync(message);

            await writer.WriteMessageAsync(message);
            _renderedMessageCount++;
        }

        public async ValueTask DisposeAsync() => await ResetWriterAsync();
    }

    internal partial class MessageExporter
    {
        private static string GetPartitionFilePath(string baseFilePath, int partitionIndex)
        {
            // First partition - don't change file name
            if (partitionIndex <= 0)
                return baseFilePath;

            // Inject partition index into file name
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFilePath);
            var fileExt = Path.GetExtension(baseFilePath);
            var fileName = $"{fileNameWithoutExt} [part {partitionIndex + 1}]{fileExt}";

            // Generate new path
            var dirPath = Path.GetDirectoryName(baseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                return Path.Combine(dirPath, fileName);

            return fileName;
        }

        private static MessageWriter CreateMessageWriter(string filePath, ExportFormat format, ExportContext context)
        {
            // Stream will be disposed by the underlying writer
            var stream = File.Create(filePath);

            return format switch
            {
                ExportFormat.PlainText => new PlainTextMessageWriter(stream, context),
                ExportFormat.Csv => new CsvMessageWriter(stream, context),
                ExportFormat.HtmlDark => new HtmlMessageWriter(stream, context, "Dark"),
                ExportFormat.HtmlLight => new HtmlMessageWriter(stream, context, "Light"),
                ExportFormat.Json => new JsonMessageWriter(stream, context),
                _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unknown export format '{format}'.")
            };
        }
    }
}