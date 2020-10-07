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

        private readonly Dictionary<string, string> _pathMap = new Dictionary<string, string>();

        public MediaDownloader(string workingDirPath)
        {
            _workingDirPath = workingDirPath;
        }

        public async ValueTask<string> DownloadAsync(string url)
        {
            if (_pathMap.TryGetValue(url, out var cachedFilePath))
                return cachedFilePath;

            var urlParser = new MediaUrlParser(url);
            var fileName = $"{urlParser.FileName}({urlParser.ParentDirectory}).{urlParser.FileExtension}";
            var filePath = Path.Combine(_workingDirPath, fileName);

            return _pathMap[url] = filePath;
        }
    }

    internal class MediaUrlParser
    {
        public string ParentDirectory { get; }
        public string FileName { get; }

        public string FileExtension { get; }
        private static string GetRandomFileName() => Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

        public MediaUrlParser(string url)
        {
            var match = Regex.Match(url, @".+/([^?]*)/([^?]*)\.([^?]*)");

            ParentDirectory = match.Groups[1].Value;
            var originalFileName = match.Groups[2].Value;
            FileExtension = match.Groups[3].Value;
            
            FileName = PathEx.EscapePath(!string.IsNullOrWhiteSpace(originalFileName)
                ? $"{Path.GetFileNameWithoutExtension(originalFileName).Truncate(50)}{Path.GetExtension(originalFileName)}"
                : GetRandomFileName());
        }
    }

}