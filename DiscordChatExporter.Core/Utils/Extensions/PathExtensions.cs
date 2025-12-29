using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class PathExtensions
{
    // Characters that are invalid on common filesystems.
    // This is a union of invalid characters from Windows (NTFS/FAT32), Linux (ext4/XFS), and macOS (HFS+/APFS).
    // We use this instead of Path.GetInvalidFileNameChars() because that only returns OS-specific characters,
    // not filesystem-specific characters. This means that it's possible to use, for example, an NTFS drive on
    // Linux, which would allow the OS to create filenames with '?' but result in errors when writing to the filesystem.
    // https://github.com/Tyrrrz/DiscordChatExporter/issues/1452
    private static readonly char[] InvalidFileNameChars =
    [
        '\0', // Null character - invalid on all filesystems
        '/', // Path separator on Unix
        '\\', // Path separator on Windows
        ':', // Reserved on Windows (drive letters, NTFS streams)
        '*', // Wildcard on Windows
        '?', // Wildcard on Windows
        '"', // Reserved on Windows
        '<', // Redirection on Windows
        '>', // Redirection on Windows
        '|', // Pipe on Windows
    ];

    extension(Path)
    {
        public static string EscapeFileName(string path)
        {
            var buffer = new StringBuilder(path.Length);

            foreach (var c in path)
                buffer.Append(!InvalidFileNameChars.Contains(c) ? c : '_');

            // File names cannot end with a dot on Windows
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/977
            if (OperatingSystem.IsWindows())
            {
                while (buffer.Length > 0 && buffer[^1] == '.')
                    buffer.Remove(buffer.Length - 1, 1);
            }

            return buffer.ToString();
        }
    }
}
