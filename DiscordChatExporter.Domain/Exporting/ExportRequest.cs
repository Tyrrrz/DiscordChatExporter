using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    public partial class ExportRequest
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public string OutputBaseDirPath { get; }

        public string OutputMediaDirPath { get; }

        public IEnumerable<ExportFormat> Formats { get; }

        public DateTimeOffset? After { get; }

        public DateTimeOffset? Before { get; }

        public int? PartitionLimit { get; }

        public bool ShouldDownloadMedia { get; }

        public string DateFormat { get; }

        public ExportRequest(
            Guild guild,
            Channel channel,
            string outputDirPath,
            IEnumerable<ExportFormat> formats,
            DateTimeOffset? after,
            DateTimeOffset? before,
            int? partitionLimit,
            bool shouldDownloadMedia,
            string dateFormat)
        {
            Guild = guild;
            Channel = channel;
            OutputBaseDirPath = AsDirectoryIfFile(outputDirPath);
            Formats = formats;
            After = after;
            Before = before;
            PartitionLimit = partitionLimit;
            ShouldDownloadMedia = shouldDownloadMedia;
            DateFormat = dateFormat;

            OutputMediaDirPath = Path.Combine(OutputBaseDirPath,
                GetOutputFileNameWithoutExtension(guild, channel, after, before)) + $"_Files{Path.DirectorySeparatorChar}";

            string AsDirectoryIfFile(string path)
            {
                if (isPathToFile(path))
                {
                    var dir = Path.GetDirectoryName(path);
                    return dir switch
                    {
                        "" => dir,
                        null => throw new ArgumentException($"Received invalid output directory: ${dir}"),
                        var val => val,
                    };
                }

                validatePath(path);

                return path;

                static void validatePath(string path) => Path.GetFullPath(path);
            }

            static bool isPathToFile(string path) => !Directory.Exists(path) && !string.IsNullOrWhiteSpace(Path.GetExtension(path));
        }

        public string GetOutputFilePathForFormat(ExportFormat format)
        {
            if (!Formats.Includes(format))
            {
                throw new ArgumentException($"Format {format} was not included in this export request.");
            }

            return GetOutputFilePath(Guild, Channel, OutputBaseDirPath, format, After, Before);
        }
    }

    public partial class ExportRequest
    {
        public static string GetDefaultOutputFileName(
            Guild guild,
            Channel channel,
            ExportFormat format,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {
            var withoutExtension = GetOutputFileNameWithoutExtension(guild, channel, after, before);
            return $"{withoutExtension}.{format.GetFileExtension()}";
        }

        private static string GetOutputFileNameWithoutExtension(
            Guild guild,
            Channel channel,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {

            var buffer = new StringBuilder();

            // Guild and channel names
            buffer.Append($"{guild.Name} - {channel.Category} - {channel.Name} [{channel.Id}]");

            // Date range
            if (after != null || before != null)
            {
                buffer.Append(" (");

                // Both 'after' and 'before' are set
                if (after != null && before != null)
                {
                    buffer.Append($"{after:yyyy-MM-dd} to {before:yyyy-MM-dd}");
                }
                // Only 'after' is set
                else if (after != null)
                {
                    buffer.Append($"after {after:yyyy-MM-dd}");
                }
                // Only 'before' is set
                else
                {
                    buffer.Append($"before {before:yyyy-MM-dd}");
                }

                buffer.Append(")");
            }

            // Replace invalid chars
            PathEx.EscapePath(buffer);

            return buffer.ToString();
        }
        private static string GetOutputFilePath(
            Guild guild,
            Channel channel,
            string outputDirPath,
            ExportFormat format,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {
            return Path.Combine(outputDirPath, GetDefaultOutputFileName(guild, channel, format, after, before));
        }
    }
}