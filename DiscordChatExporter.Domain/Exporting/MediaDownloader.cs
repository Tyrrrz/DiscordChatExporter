using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    internal class MediaDownloader
    {
        private readonly HttpClient _httpClient = Singleton.HttpClient;
        private readonly string _workingDirPath;

        private readonly Dictionary<string, string> _pathMap = new Dictionary<string, string>();

        public MediaDownloader(string workingDirPath)
        {
            _workingDirPath = workingDirPath;
        }

        private string GetRandomSuffix() => Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);

        private string GetFileNameFromUrl(string url)
        {
            var originalFileName = Regex.Match(url, @".+/([^?]*)").Groups[1].Value;

            if (string.IsNullOrWhiteSpace(originalFileName))
                return GetRandomSuffix();

            return $"{Path.GetFileNameWithoutExtension(originalFileName)}-{GetRandomSuffix()}{Path.GetExtension(originalFileName)}";
        }

        // HACK: ConfigureAwait() is crucial here to enable sync-over-async in HtmlMessageWriter
        public async ValueTask<string> DownloadAsync(string url)
        {
            if (_pathMap.TryGetValue(url, out var cachedFilePath))
                return cachedFilePath;

            var fileName = GetFileNameFromUrl(url);
            var filePath = Path.Combine(_workingDirPath, fileName);

            Directory.CreateDirectory(_workingDirPath);

            await _httpClient.DownloadAsync(url, filePath).ConfigureAwait(false);

            return _pathMap[url] = filePath;
        }
    }
}