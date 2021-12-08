using System.IO;
using System.Text;

namespace DiscordChatExporter.Core.Utils;

public static class PathEx
{
    public static StringBuilder EscapePath(StringBuilder pathBuffer)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            pathBuffer.Replace(invalidChar, '_');

        return pathBuffer;
    }

    public static string EscapePath(string path) => EscapePath(new StringBuilder(path)).ToString();
}