﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using DiscordChatExporter.Core.Utils;

namespace DiscordChatExporter.Core.Exporting;

public partial class ExportRequest
{
    public Guild Guild { get; }

    public IChannel Channel { get; }

    public string OutputFilePath { get; }

    public string OutputDirPath { get; }

    public string AssetsDirPath { get; }

    public ExportFormat Format { get; }

    public Snowflake? After { get; }

    public Snowflake? Before { get; }

    public PartitionLimit PartitionLimit { get; }

    public MessageFilter MessageFilter { get; }

    public bool ShouldFormatMarkdown { get; }

    public bool ShouldDownloadAssets { get; }

    public bool ShouldReuseAssets { get; }

    public string DateFormat { get; }

    public ExportRequest(
        Guild guild,
        IChannel channel,
        string outputPath,
        string? assetsDirPath,
        ExportFormat format,
        Snowflake? after,
        Snowflake? before,
        PartitionLimit partitionLimit,
        MessageFilter messageFilter,
        bool shouldFormatMarkdown,
        bool shouldDownloadAssets,
        bool shouldReuseAssets,
        string dateFormat)
    {
        Guild = guild;
        Channel = channel;
        Format = format;
        After = after;
        Before = before;
        PartitionLimit = partitionLimit;
        MessageFilter = messageFilter;
        ShouldFormatMarkdown = shouldFormatMarkdown;
        ShouldDownloadAssets = shouldDownloadAssets;
        ShouldReuseAssets = shouldReuseAssets;
        DateFormat = dateFormat;

        OutputFilePath = GetOutputBaseFilePath(
            Guild,
            Channel,
            outputPath,
            Format,
            After,
            Before
        );

        OutputDirPath = Path.GetDirectoryName(OutputFilePath)!;

        AssetsDirPath = !string.IsNullOrWhiteSpace(assetsDirPath)
            ? FormatPath(
                assetsDirPath,
                Guild,
                Channel,
                After,
                Before
            )
            : $"{OutputFilePath}_Files{Path.DirectorySeparatorChar}";
    }
}

public partial class ExportRequest
{
    public static string GetDefaultOutputFileName(
        Guild guild,
        IChannel channel,
        ExportFormat format,
        Snowflake? after = null,
        Snowflake? before = null)
    {
        var buffer = new StringBuilder();

        // Guild and channel names
        buffer.Append($"{guild.Name} - {channel.ParentName} - {channel.Name} [{channel.Id}]");

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

    private static string FormatPath(
        string path,
        Guild guild,
        IChannel channel,
        Snowflake? after,
        Snowflake? before)
    {
        return Regex.Replace(
            path,
            "%.",
            m => PathEx.EscapeFileName(m.Value switch
            {
                "%g" => guild.Id.ToString(),
                "%G" => guild.Name,
                "%t" => channel.ParentId.ToString(),
                "%T" => channel.ParentName,
                "%c" => channel.Id.ToString(),
                "%C" => channel.Name,
                "%p" => channel.Position?.ToString() ?? "0",
                "%P" => channel.ParentPosition?.ToString() ?? "0",
                "%a" => after?.ToDate().ToString("yyyy-MM-dd") ?? "",
                "%b" => before?.ToDate().ToString("yyyy-MM-dd") ?? "",
                "%d" => DateTimeOffset.Now.ToString("yyyy-MM-dd"),
                "%%" => "%",
                _ => m.Value
            })
        );
    }

    private static string GetOutputBaseFilePath(
        Guild guild,
        IChannel channel,
        string outputPath,
        ExportFormat format,
        Snowflake? after = null,
        Snowflake? before = null)
    {
        var actualOutputPath = FormatPath(outputPath, guild, channel, after, before);

        // Output is a directory
        if (Directory.Exists(actualOutputPath) || string.IsNullOrWhiteSpace(Path.GetExtension(actualOutputPath)))
        {
            var fileName = GetDefaultOutputFileName(guild, channel, format, after, before);
            return Path.Combine(actualOutputPath, fileName);
        }

        // Output is a file
        return actualOutputPath;
    }
}