using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private static readonly AsyncKeyedLocker<string> Locker = new();

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
                await using var output = File.Create(filePath);
                await response.Content.CopyToAsync(output, innerCancellationToken);
            },
            cancellationToken
        );

        return _previousPathsByUrl[url] = filePath;
    }
}

internal partial class ExportAssetDownloader
{
    private const String CHARSET = "0123456789bcdfghjklmnpqrstvwxyz_";

    private static String Base32(byte[] data)
    {
        var newString = new StringBuilder();
        uint accum = 0;
        uint bits = 0;

        foreach (byte b in data)
        {
            accum <<= 8;
            accum |= b;
            bits += 8;

            while (bits > 5)
            {
                char ch = CHARSET[(int)(accum & 0x1F)];
                accum >>= 5;
                bits -= 5;
                newString.Append(ch);
            }
        }
        if (bits != 0)
        {
            char ch = CHARSET[(int)(accum & 0x1F)];
            newString.Append(ch);
        }

        return newString.ToString();
    }

    private static string GetUrlHash(string url)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        // 12 characters of base32 contains about as much entropy as a Discord snowflake
        return Base32(hash).Truncate(12);
    }

    private static string AddHashToUrl(string url, string urlHash)
    {
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

    private static string GetFileNameFromUrl(string url)
    {
        var uri = new Uri(url);

        if (string.Equals(uri.Host, "cdn.discordapp.com"))
        {
            string[] split = uri.AbsolutePath.Split("/");

            // Attachments
            if (uri.AbsolutePath.StartsWith("/attachments/") && split.Length == 5)
            {
                // use the attachment snowflake for attachments
                if (ulong.TryParse(split[3], out var snowflake))
                    return AddHashToUrl(url, snowflake.ToString());
            }

            // Emojis
            if (
                uri.AbsolutePath.StartsWith("/emojis/")
                && split.Length == 3
                && split[2].Contains(".")
            )
            {
                var nameSplit = split[2].Split(".", 2);
                if (ulong.TryParse(nameSplit[0], out var snowflake))
                    return $"emoji-discord-{snowflake}.{nameSplit[1]}";
            }

            // Avatars
            if (uri.AbsolutePath.StartsWith("/avatars/") && split.Length == 4)
            {
                return $"avatar-{split[2]}-{GetUrlHash(url)}.{split[3].Split(".").Last()}";
            }
        }

        if (string.Equals(uri.Host, "cdn.jsdelivr.net"))
        {
            string[] split = uri.AbsolutePath.Split("/");

            // twemoji
            if (
                uri.AbsolutePath.StartsWith("/gh/twitter/twemoji@latest/assets/svg/")
                && split.Length == 7
            )
            {
                return $"emoji-twemoji-{split[6]}";
            }
        }

        return AddHashToUrl(url, GetUrlHash(url));
    }
}
