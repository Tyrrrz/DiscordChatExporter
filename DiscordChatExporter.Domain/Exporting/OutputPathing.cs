using System;
using System.IO;

namespace DiscordChatExporter.Domain.Exporting
{
    public partial class OutputPathing
    {

        public string DirectoryPath { get; }
        public string? FilenameWithoutExt { get; }

        public OutputPathing(string directoryPath, string? filenameWithoutExt = null)
        {
            DirectoryPath = directoryPath;
            FilenameWithoutExt = filenameWithoutExt;
        }
    }

    public partial class OutputPathing
    {
        public static OutputPathing ParseForMultipleOutputFormats(string outputPath)
        {
            if (isFilePath(outputPath))
            {
                throw new ArgumentException("Output path cannot include a file name when exporting to multiple formats.");
            }

            return new OutputPathing(outputPath);
        }

        public static OutputPathing ParseForSingleOutputFormat(string outputPath)
        {
            if (isDirectoryPath(outputPath))
            {
                return new OutputPathing(outputPath);
            }
            else // Path is to File.
            {
                var dirPath = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrEmpty(dirPath))
                {
                    throw new ArgumentException($"Received invalid directory path: {dirPath}");
                }

                return new OutputPathing(dirPath, Path.GetFileNameWithoutExtension(outputPath));
            }
        }

        private static bool isDirectoryPath(string path) => string.IsNullOrEmpty(Path.GetExtension(path));
        private static bool isFilePath(string path) => !isDirectoryPath(path);

    }

}
