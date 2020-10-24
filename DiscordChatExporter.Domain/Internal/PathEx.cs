using System.IO;
using System.Text;

namespace DiscordChatExporter.Domain.Internal
{
    internal static class PathEx
    {
        public static StringBuilder EscapePath(StringBuilder pathBuffer)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                pathBuffer.Replace(invalidChar, '_');

            return pathBuffer;
        }

        public static string EscapePath(string path) => EscapePath(new StringBuilder(path)).ToString();
    }
}