using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

public partial class ExportRequest
{
    public Guild Guild { get; }

    public Channel Channel { get; }

    public string OutputFilePath { get; }

    public string OutputDirPath { get; }

    public string AssetsDirPath { get; }

    public ExportFormat Format { get; }

    public Snowflake? After { get; }

    public Snowflake? Before { get; }

    public ExportExistsHandling ExportExistsHandling { get; }

    public Snowflake? LastPriorMessage { get; set; }

    public PartitionLimit PartitionLimit { get; }

    public MessageFilter MessageFilter { get; }

    public bool ShouldFormatMarkdown { get; }

    public bool ShouldDownloadAssets { get; }

    public bool ShouldReuseAssets { get; }

    public string? Locale { get; }

    public CultureInfo? CultureInfo { get; }

    public bool IsUtcNormalizationEnabled { get; }

    public ExportRequest(
        Guild guild,
        Channel channel,
        string outputPath,
        string? assetsDirPath,
        ExportFormat format,
        Snowflake? after,
        Snowflake? before,
        ExportExistsHandling exportExistsHandling,
        PartitionLimit partitionLimit,
        MessageFilter messageFilter,
        bool shouldFormatMarkdown,
        bool shouldDownloadAssets,
        bool shouldReuseAssets,
        string? locale,
        bool isUtcNormalizationEnabled
    )
    {
        Guild = guild;
        Channel = channel;
        Format = format;
        After = after;
        Before = before;
        ExportExistsHandling = exportExistsHandling;
        PartitionLimit = partitionLimit;
        MessageFilter = messageFilter;
        ShouldFormatMarkdown = shouldFormatMarkdown;
        ShouldDownloadAssets = shouldDownloadAssets;
        ShouldReuseAssets = shouldReuseAssets;
        Locale = locale;
        IsUtcNormalizationEnabled = isUtcNormalizationEnabled;

        OutputFilePath = GetOutputBaseFilePath(Guild, Channel, outputPath, Format, After, Before);

        OutputDirPath = Path.GetDirectoryName(OutputFilePath)!;

        AssetsDirPath = !string.IsNullOrWhiteSpace(assetsDirPath)
            ? FormatPath(assetsDirPath, Guild, Channel, After, Before)
            : $"{OutputFilePath}_Files{Path.DirectorySeparatorChar}";

        CultureInfo = Locale?.Pipe(CultureInfo.GetCultureInfo);
    }
}

public partial class ExportRequest
{
    public static string GetDefaultOutputFileName(
        Guild guild,
        Channel channel,
        ExportFormat format,
        Snowflake? after = null,
        Snowflake? before = null
    )
    {
        // Do not change this without adding the new version to the corresponding regex below
        var buffer = new StringBuilder();

        // Guild name
        buffer.Append(guild.Name);

        // Parent name
        if (channel.Parent is not null)
            buffer.Append(" - ").Append(channel.Parent.Name);

        // Channel name and ID
        buffer
            .Append(" - ")
            .Append(channel.Name)
            .Append(' ')
            .Append('[')
            .Append(channel.Id)
            .Append(']');

        // Date range
        if (after is not null || before is not null)
        {
            buffer.Append(' ').Append('(');

            // Both 'after' and 'before' are set
            if (after is not null && before is not null)
            {
                buffer.Append(
                    $"{after.Value.ToDate():yyyy-MM-dd} to {before.Value.ToDate():yyyy-MM-dd}"
                );
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

    /// <summary>
    /// Returns a regex that matches any default file name this channel export might have had in the past.
    /// This can be used to detect existing exports of this channel with a different guild, parent and / or channel
    /// name.
    /// This only matches existing exports with the same date range as the current export.
    /// </summary>
    /// <returns>A regex that matches any default file name this channel might have had in the past.</returns>
    public Regex GetDefaultOutputFileNameRegex()
    {
        // While this code looks similar to GetDefaultOutputFileName, the two functions are intentionally independent
        // Even if the default output file name gets changed, the previous default file names should still be matched
        // by this; the new version should just be added additionally to this regex
        var buffer = new StringBuilder();

        // Guild name
        buffer.Append(".*?");

        // Parent name
        if (Channel.Parent is not null)
            buffer.Append(" - ").Append(".*?");

        // Channel name and ID
        buffer
            .Append(" - ")
            .Append(".*?")
            .Append(' ')
            .Append("\\[")
            .Append(Channel.Id)
            .Append("\\]");

        // Date range
        if (After is not null || Before is not null)
        {
            buffer.Append(' ').Append("\\(");
            if (After is not null && Before is not null)
            {
                buffer.Append(
                    $"{After.Value.ToDate():yyyy-MM-dd} to {Before.Value.ToDate():yyyy-MM-dd}"
                );
            }
            else if (After is not null)
            {
                buffer.Append($"after {After.Value.ToDate():yyyy-MM-dd}");
            }
            else if (Before is not null)
            {
                buffer.Append($"before {Before.Value.ToDate():yyyy-MM-dd}");
            }
            buffer.Append("\\)");
        }

        // File extension
        buffer.Append("\\.").Append(Format.GetFileExtension());

        return new Regex(buffer.ToString());
    }

    private static string FormatPath(
        string path,
        Guild guild,
        Channel channel,
        Snowflake? after,
        Snowflake? before
    ) =>
        Regex.Replace(
            path,
            "%.",
            m =>
                PathEx.EscapeFileName(
                    m.Value switch
                    {
                        "%g" => guild.Id.ToString(),
                        "%G" => guild.Name,

                        "%t" => channel.Parent?.Id.ToString() ?? "",
                        "%T" => channel.Parent?.Name ?? "",

                        "%c" => channel.Id.ToString(),
                        "%C" => channel.Name,

                        "%p" => channel.Position?.ToString(CultureInfo.InvariantCulture) ?? "0",
                        "%P" => channel.Parent?.Position?.ToString(CultureInfo.InvariantCulture)
                            ?? "0",

                        "%a" => after?.ToDate().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            ?? "",
                        "%b" => before
                            ?.ToDate()
                            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
                        "%d" => DateTimeOffset.Now.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture
                        ),

                        "%%" => "%",
                        _ => m.Value,
                    }
                )
        );

    private static string GetOutputBaseFilePath(
        Guild guild,
        Channel channel,
        string outputPath,
        ExportFormat format,
        Snowflake? after = null,
        Snowflake? before = null
    )
    {
        var actualOutputPath = FormatPath(outputPath, guild, channel, after, before);

        // Output is a directory
        if (
            Directory.Exists(actualOutputPath)
            || string.IsNullOrWhiteSpace(Path.GetExtension(actualOutputPath))
        )
        {
            var fileName = GetDefaultOutputFileName(guild, channel, format, after, before);
            return Path.Combine(actualOutputPath, fileName);
        }

        // Output is a file
        return actualOutputPath;
    }
}
