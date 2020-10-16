using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;
using Polly;
using Polly.Retry;

namespace DiscordChatExporter.Domain.Exporting
{
    internal partial class MediaDownloader
    {
        private readonly HttpClient _httpClient = Singleton.HttpClient;
        private readonly string _workingDirPath;
        private readonly AsyncRetryPolicy _httpRequestPolicy;

        private readonly Dictionary<string, string> _pathMap = new Dictionary<string, string>();

        public MediaDownloader(string workingDirPath)
        {
            _workingDirPath = workingDirPath;

            _httpRequestPolicy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(8, i => TimeSpan.FromSeconds(0.5 * i));
        }

        public async ValueTask<string> DownloadAsync(string url)
        {
            return await _httpRequestPolicy.ExecuteAsync(async () =>
            {
                if (_pathMap.TryGetValue(url, out var cachedFilePath))
                    return cachedFilePath;

                var fileName = GetFileNameFromUrl(url);
                var filePath = PathEx.MakeUniqueFilePath(Path.Combine(_workingDirPath, fileName));

                Directory.CreateDirectory(_workingDirPath);

                _pathMap[url] = filePath;

                await _httpClient.DownloadAsync(url, filePath);

                return filePath;
            });
        }
    }

    internal partial class MediaDownloader
    {
        private static string GetRandomFileName() => Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

        private static string GetFileNameFromUrl(string url)
        {
            var originalFileName = Regex.Match(url, @".+/([^?]*)").Groups[1].Value;

            var fileName = !string.IsNullOrWhiteSpace(originalFileName)
                ? $"{Path.GetFileNameWithoutExtension(originalFileName).Truncate(50)}{Path.GetExtension(originalFileName)}"
                : GetRandomFileName();

            return PathEx.EscapePath(fileName);
        }
    }
}