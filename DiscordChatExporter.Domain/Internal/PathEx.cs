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

        public static string MakeUniqueFilePath(string baseFilePath, int maxAttempts = 100)
        {
            if (!File.Exists(baseFilePath))
                return baseFilePath;

            var baseDirPath = Path.GetDirectoryName(baseFilePath);
            var baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
            var baseFileExtension = Path.GetExtension(baseFilePath);

            for (var i = 1; i <= maxAttempts; i++)
            {
                var filePath = $"{baseFileNameWithoutExtension} ({i}){baseFileExtension}";
                if (!string.IsNullOrWhiteSpace(baseDirPath))
                    filePath = Path.Combine(baseDirPath, filePath);

                if (!File.Exists(filePath))
                    return filePath;
            }

            return baseFilePath;
        }
    }
}