using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordChatExporter.Core.Utils;

public static class PathEx
{
    private static readonly HashSet<char> InvalidFileNameChars =
    [
        .. Path.GetInvalidFileNameChars(),
    ];

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
