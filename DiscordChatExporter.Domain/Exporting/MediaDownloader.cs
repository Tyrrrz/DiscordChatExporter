using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    internal partial class MediaDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly string _workingDirPath;
        private readonly bool _reuseMedia;

        // URL -> Local file path
        private readonly Dictionary<string, string> _pathCache =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public MediaDownloader(HttpClient httpClient, string workingDirPath, bool reuseMedia)
        {
            _httpClient = httpClient;
            _workingDirPath = workingDirPath;
            _reuseMedia = reuseMedia;
        }

        public MediaDownloader(string workingDirPath, bool reuseMedia)
            : this(Http.Client, workingDirPath, reuseMedia) {}

        public async ValueTask<string> DownloadAsync(string url)
        {
            if (_pathCache.TryGetValue(url, out var cachedFilePath))
                return cachedFilePath;

            var fileName = GetFileNameFromUrl(url);
            var filePath = Path.Combine(_workingDirPath, fileName);

            // Reuse existing files if we're allowed to
            if (_reuseMedia && File.Exists(filePath))
                return _pathCache[url] = filePath;

            // Download it
            Directory.CreateDirectory(_workingDirPath);
            await Http.ExceptionPolicy.ExecuteAsync(async () =>
            {
                // This catches IOExceptions which is dangerous as we're working also with files
                await _httpClient.DownloadAsync(url, filePath);
            });

            return _pathCache[url] = filePath;
        }
    }

    internal partial class MediaDownloader
    {
        private static string GetUrlHash(string url)
        {
            using var hash = SHA256.Create();

            var data = hash.ComputeHash(Encoding.UTF8.GetBytes(url));
            return data.ToHex().Truncate(5); // 5 chars ought to be enough for anybody
        }

        private static string GetFileNameFromUrl(string url)
        {
            var urlHash = GetUrlHash(url);

            // Try to extract file name from URL
            var fileName = Regex.Match(url, @".+/([^?]*)").Groups[1].Value;

            // If it's not there, just use the URL hash as the file name
            if (string.IsNullOrWhiteSpace(fileName))
                return urlHash;

            // Otherwise, use the original file name but inject the hash in the middle
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var fileExtension = Path.GetExtension(fileName);

            return PathEx.EscapePath(fileNameWithoutExtension.Truncate(42) + '-' + urlHash + fileExtension);
        }
    }
}
