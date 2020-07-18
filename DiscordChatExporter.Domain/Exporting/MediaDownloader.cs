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
    internal partial class MediaDownloader
    {
        private readonly HttpClient _httpClient = Singleton.HttpClient;
        private readonly string _workingDirPath;

        private readonly Dictionary<string, string> _mediaPathMap = new Dictionary<string, string>();

        public MediaDownloader(string workingDirPath)
        {
            _workingDirPath = workingDirPath;
        }

        // HACK: ConfigureAwait() is crucial here to enable sync-over-async in HtmlMessageWriter
        public async ValueTask<string> DownloadAsync(string url)
        {
            if (_mediaPathMap.TryGetValue(url, out var cachedFilePath))
                return cachedFilePath;

            var extension = Path.GetExtension(GetFileNameFromUrl(url));
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_workingDirPath, fileName);

            Directory.CreateDirectory(_workingDirPath);

            await _httpClient.DownloadAsync(url, filePath).ConfigureAwait(false);

            return _mediaPathMap[url] = filePath;
        }
    }

    internal partial class MediaDownloader
    {
        private static string GetFileNameFromUrl(string url) => Regex.Match(url, @".+/([^?]*)").Groups[1].Value;
    }
}