using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    internal class UrlProcessor
    {
        private readonly HttpClient _httpClient = Singleton.HttpClient;
        private readonly string _outputDirPath;

        private readonly Dictionary<string, string> _mediaPathMap = new Dictionary<string, string>();

        public UrlProcessor(string outputDirPath)
        {
            _outputDirPath = outputDirPath;
        }

        // HACK: ConfigureAwait() is crucial here to enable sync-over-async in HtmlMessageWriter
        public async Task<string> ConvertAsync(string url)
        {
            if (_mediaPathMap.TryGetValue(url, out var cachedFilePath))
                return cachedFilePath;

            Directory.CreateDirectory(_outputDirPath);

            var extension = Path.GetExtension(new Uri(url).LocalPath);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var absoluteFilePath = Path.Combine(_outputDirPath, fileName);

            await _httpClient.DownloadAsync(url, absoluteFilePath).ConfigureAwait(false);
            var relativeFilePath = $"{Path.GetFileName(Path.GetDirectoryName(_outputDirPath))}/{fileName}";

            return _mediaPathMap[url] = relativeFilePath;
        }
    }
}