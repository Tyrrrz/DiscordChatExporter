using System;
using System.Text;

namespace DiscordChatExporter.Core.Utils;

public static class Url
{
    public static string EncodeFilePath(string filePath)
    {
        var buffer = new StringBuilder();
        var position = 0;

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
