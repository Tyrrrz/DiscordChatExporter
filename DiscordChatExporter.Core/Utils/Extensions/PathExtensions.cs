using System;
using System.IO;
using System.Text;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class PathExtensions
{
    extension(Path)
    {
        public static string EscapeFileName(string path)
        {
            var buffer = new StringBuilder(path.Length);

            foreach (var c in path)
                buffer.Append(!Path.GetInvalidFileNameChars().Contains(c) ? c : '_');

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
