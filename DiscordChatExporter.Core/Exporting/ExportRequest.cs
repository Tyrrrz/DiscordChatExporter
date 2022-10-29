using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Exporting;

public partial record ExportRequest(
    Guild Guild,
    Channel Channel,
    string OutputPath,
    ExportFormat Format,
    Snowflake? After,
    Snowflake? Before,
    PartitionLimit PartitionLimit,
    MessageFilter MessageFilter,
    bool ShouldDownloadAssets,
    bool ShouldReuseAssets,
    string DateFormat)
{
    private string? _outputBaseFilePath;
    public string OutputBaseFilePath => _outputBaseFilePath ??= GetOutputBaseFilePath(
        Guild,
        Channel,
        OutputPath,
        Format,
        After,
        Before
    );

    public string OutputBaseDirPath => Path.GetDirectoryName(OutputBaseFilePath) ?? OutputPath;

    public string OutputAssetsDirPath => $"{OutputBaseFilePath}_Files{Path.DirectorySeparatorChar}";
}

public partial record ExportRequest
{
    private static string GetOutputBaseFilePath(
        Guild guild,
        Channel channel,
        string outputPath,
        ExportFormat format,
        Snowflake? after = null,
        Snowflake? before = null)
    {
        // Formats path
        outputPath = Regex.Replace(outputPath, "%.", m =>
            PathEx.EscapeFileName(m.Value switch
            {
                "%g" => guild.Id.ToString(),
                "%G" => guild.Name,
                "%t" => channel.Category.Id.ToString(),
                "%T" => channel.Category.Name,
                "%c" => channel.Id.ToString(),
                "%C" => channel.Name,
                "%p" => channel.Position?.ToString() ?? "0",
                "%P" => channel.Category.Position?.ToString() ?? "0",
                "%a" => after?.ToDate().ToString("yyyy-MM-dd") ?? "",
                "%b" => before?.ToDate().ToString("yyyy-MM-dd") ?? "",
                "%d" => DateTimeOffset.Now.ToString("yyyy-MM-dd"),
                "%%" => "%",
                _ => m.Value
            })
        );

        // Output is a directory
        if (Directory.Exists(outputPath) || string.IsNullOrWhiteSpace(Path.GetExtension(outputPath)))
        {
            var fileName = GetDefaultOutputFileName(guild, channel, format, after, before);
            return Path.Combine(outputPath, fileName);
        }

        // Output is a file
        return outputPath;
    }

    public static string GetDefaultOutputFileName(
        Guild guild,
        Channel channel,
        ExportFormat format,
        Snowflake? after = null,
        Snowflake? before = null)
    {
        var buffer = new StringBuilder();

        // Guild and channel names
        buffer.Append($"{guild.Name} - {channel.Category.Name} - {channel.Name} [{channel.Id}]");

        // Date range
        if (after is not null || before is not null)
        {
            buffer.Append(' ').Append('(');

            // Both 'after' and 'before' are set
            if (after is not null && before is not null)
            {
                buffer.Append($"{after.Value.ToDate():yyyy-MM-dd} to {before.Value.ToDate():yyyy-MM-dd}");
            }
            // Only 'after' is set
            else if (after is not null)
            {
                buffer.Append($"after {after.Value.ToDate():yyyy-MM-dd}");
            }
            // Only 'before' is set
            else if (before is not null)
            {
                buffer.Append($"before {before.Value.ToDate():yyyy-MM-dd}");
            }

            buffer.Append(')');
        }

        // File extension
        buffer.Append('.').Append(format.GetFileExtension());

        return PathEx.EscapeFileName(buffer.ToString());
    }
}