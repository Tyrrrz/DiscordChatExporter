using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AsyncKeyedLock;
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal partial class ExportAssetDownloader(string workingDirPath, bool reuse)
{
    private static readonly AsyncKeyedLocker<string> Locker =
        new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

    // File paths of the previously downloaded assets
    private readonly Dictionary<string, string> _previousPathsByUrl = new(StringComparer.Ordinal);

    public async ValueTask<string> DownloadAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        var fileName = GetFileNameFromUrl(url);
        var filePath = Path.Combine(workingDirPath, fileName);

        using var _ = await Locker.LockAsync(filePath, cancellationToken);

        if (_previousPathsByUrl.TryGetValue(url, out var cachedFilePath))
            return cachedFilePath;

        // Reuse existing files if we're allowed to
        if (reuse && File.Exists(filePath))
            return _previousPathsByUrl[url] = filePath;

        Directory.CreateDirectory(workingDirPath);

        await Http.ResiliencePipeline.ExecuteAsync(
            async innerCancellationToken =>
            {
                // Download the file
                using var response = await Http.Client.GetAsync(url, innerCancellationToken);
                await using (var output = File.Create(filePath))
                    await response.Content.CopyToAsync(output, innerCancellationToken);

                // Try to set the file date according to the last-modified header
                try
                {
                    var lastModified = response
                        .Content.Headers.TryGetValue("Last-Modified")
                        ?.Pipe(s =>
                            DateTimeOffset.TryParse(
                                s,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out var instant
                            )
                                ? instant
                                : (DateTimeOffset?)null
                        );

                    if (lastModified is not null)
                    {
                        File.SetCreationTimeUtc(filePath, lastModified.Value.UtcDateTime);
                        File.SetLastWriteTimeUtc(filePath, lastModified.Value.UtcDateTime);
                        File.SetLastAccessTimeUtc(filePath, lastModified.Value.UtcDateTime);
                    }
                }
                catch
                {
                    // This can apparently fail for some reason.
                    // Updating the file date is not a critical task, so we'll just ignore exceptions thrown here.
                    // https://github.com/Tyrrrz/DiscordChatExporter/issues/585
                }
            },
            cancellationToken
        );

        return _previousPathsByUrl[url] = filePath;
    }
}

internal partial class ExportAssetDownloader
{
    private static string GetUrlHash(string url)
    {
        // Remove signature parameters from Discord CDN URLs to normalize them
        static string NormalizeUrl(string url)
        {
            var uri = new Uri(url);
            if (!string.Equals(uri.Host, "cdn.discordapp.com", StringComparison.OrdinalIgnoreCase))
                return url;

            var query = HttpUtility.ParseQueryString(uri.Query);
            query.Remove("ex");
            query.Remove("is");
            query.Remove("hm");

            return uri.GetLeftPart(UriPartial.Path) + query;
        }

        return SHA256
            .HashData(Encoding.UTF8.GetBytes(NormalizeUrl(url)))
            .ToHex()
            // 5 chars ought to be enough for anybody
            .Truncate(5);
    }

    private static string GetFileNameFromUrl(string url)
    {
        var urlHash = GetUrlHash(url);

        // Try to extract the file name from URL
        var fileName = Regex.Match(url, @".+/([^?]*)").Groups[1].Value;

        // If it's not there, just use the URL hash as the file name
        if (string.IsNullOrWhiteSpace(fileName))
            return urlHash;

        // Otherwise, use the original file name but inject the hash in the middle
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var fileExtension = Path.GetExtension(fileName);

        // Probably not a file extension, just a dot in a long file name
        // https://github.com/Tyrrrz/DiscordChatExporter/pull/812
        if (fileExtension.Length > 41)
        {
            fileNameWithoutExtension = fileName;
            fileExtension = "";
        }

        return PathEx.EscapeFileName(
            fileNameWithoutExtension.Truncate(42) + '-' + urlHash + fileExtension
        );
    }
}
