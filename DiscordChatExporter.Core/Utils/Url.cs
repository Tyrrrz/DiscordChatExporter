using System;
using System.IO;
using System.Text;

namespace DiscordChatExporter.Core.Utils;

public static class Url
{
    public static string EncodeFilePath(string filePath)
    {
        var buffer = new StringBuilder();
        var position = 0;

        // For absolute paths, prepend file:// protocol for proper browser handling
        if (Path.IsPathFullyQualified(filePath))
        {
            buffer.Append("file://");

            // On Windows, we need to add an extra slash before the drive letter
            // e.g., file:///C:/path instead of file://C:/path
            if (!filePath.StartsWith('/') && !filePath.StartsWith('\\'))
            {
                buffer.Append('/');
            }
        }

        while (true)
        {
            if (position >= filePath.Length)
                break;

            var separatorIndex = filePath.IndexOfAny([':', '/', '\\'], position);
            if (separatorIndex < 0)
            {
                buffer.Append(Uri.EscapeDataString(filePath[position..]));
                break;
            }

            // Append the segment
            buffer.Append(Uri.EscapeDataString(filePath[position..separatorIndex]));

            // Append the separator
            buffer.Append(
                filePath[separatorIndex] switch
                {
                    // Normalize slashes
                    '\\' => '/',
                    var c => c,
                }
            );

            position = separatorIndex + 1;
        }

        return buffer.ToString();
    }
}
