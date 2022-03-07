using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using Gress;

namespace DiscordChatExporter.Cli.Commands.Base;

public abstract class ExportCommandBase : TokenCommandBase
{
    [CommandOption("output", 'o', Description = "Output file or directory path.")]
    public string OutputPath { get; init; } = Directory.GetCurrentDirectory();

    [CommandOption("format", 'f', Description = "Export format.")]
    public ExportFormat ExportFormat { get; init; } = ExportFormat.HtmlDark;

    [CommandOption("after", Description = "Only include messages sent after this date or message ID.")]
    public Snowflake? After { get; init; }

    [CommandOption("before", Description = "Only include messages sent before this date or message ID.")]
    public Snowflake? Before { get; init; }

    [CommandOption("partition", 'p', Description = "Split output into partitions, each limited to this number of messages (e.g. '100') or file size (e.g. '10mb').")]
    public PartitionLimit PartitionLimit { get; init; } = PartitionLimit.Null;

    [CommandOption("filter", Description = "Only include messages that satisfy this filter (e.g. 'from:foo#1234' or 'has:image').")]
    public MessageFilter MessageFilter { get; init; } = MessageFilter.Null;

    [CommandOption("parallel", Description = "Limits how many channels can be exported in parallel.")]
    public int ParallelLimit { get; init; } = 1;

    [CommandOption("media", Description = "Download referenced media content.")]
    public bool ShouldDownloadMedia { get; init; }

    [CommandOption("reuse-media", Description = "Reuse already existing media content to skip redundant downloads.")]
    public bool ShouldReuseMedia { get; init; }

    [CommandOption("dateformat", Description = "Format used when writing dates.")]
    public string DateFormat { get; init; } = "dd-MMM-yy hh:mm tt";

    private ChannelExporter? _channelExporter;
    protected ChannelExporter Exporter => _channelExporter ??= new ChannelExporter(Discord);

    protected async ValueTask ExecuteAsync(IConsole console, IReadOnlyList<Channel> channels)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        if (ShouldReuseMedia && !ShouldDownloadMedia)
        {
            throw new CommandException("Option --reuse-media cannot be used without --media.");
        }

        var errors = new ConcurrentDictionary<Channel, string>();

        // Export
        await console.Output.WriteLineAsync($"Exporting {channels.Count} channel(s)...");
        await console.CreateProgressTicker().StartAsync(async progressContext =>
        {
            await Parallel.ForEachAsync(
                channels,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, ParallelLimit),
                    CancellationToken = cancellationToken
                },
                async (channel, innerCancellationToken) =>
                {
                    try
                    {
                        await progressContext.StartTaskAsync($"{channel.Category.Name} / {channel.Name}",
                            async progress =>
                            {
                                var guild = await Discord.GetGuildAsync(channel.GuildId, innerCancellationToken);

                                var request = new ExportRequest(
                                    guild,
                                    channel,
                                    OutputPath,
                                    ExportFormat,
                                    After,
                                    Before,
                                    PartitionLimit,
                                    MessageFilter,
                                    ShouldDownloadMedia,
                                    ShouldReuseMedia,
                                    DateFormat
                                );

                                await Exporter.ExportChannelAsync(
                                    request,
                                    progress.ToPercentageBased(),
                                    innerCancellationToken
                                );
                            }
                        );
                    }
                    catch (DiscordChatExporterException ex) when (!ex.IsFatal)
                    {
                        errors[channel] = ex.Message;
                    }
                }
            );
        });

        // Print result
        using (console.WithForegroundColor(ConsoleColor.White))
        {
            await console.Output.WriteLineAsync(
                $"Successfully exported {channels.Count - errors.Count} channel(s)."
            );
        }

        // Print errors
        if (errors.Any())
        {
            await console.Output.WriteLineAsync();

            using (console.WithForegroundColor(ConsoleColor.Red))
            {
                await console.Output.WriteLineAsync(
                    $"Failed to export {errors.Count} channel(s):"
                );
            }

            foreach (var (channel, error) in errors)
            {
                await console.Output.WriteAsync($"{channel.Category.Name} / {channel.Name}: ");

                using (console.WithForegroundColor(ConsoleColor.Red))
                    await console.Output.WriteLineAsync(error);
            }

            await console.Output.WriteLineAsync();
        }

        // Fail the command only if ALL channels failed to export.
        // Having some of the channels fail to export is expected.
        if (errors.Count >= channels.Count)
        {
            throw new CommandException("Export failed.");
        }
    }

    protected async ValueTask ExecuteAsync(IConsole console, IReadOnlyList<Snowflake> channelIds)
    {
        var cancellationToken = console.RegisterCancellationHandler();
        var channels = new List<Channel>();

        foreach (var channelId in channelIds)
        {
            var channel = await Discord.GetChannelAsync(channelId, cancellationToken);
            channels.Add(channel);
        }

        await ExecuteAsync(console, channels);
    }

    public override ValueTask ExecuteAsync(IConsole console)
    {
        // War in Ukraine message
        console.Output.WriteLine("========================================================================");
        console.Output.WriteLine("||   Ukraine is at war! Support my country in its fight for freedom   ||");
        console.Output.WriteLine("||   Learn more: https://tyrrrz.me                                    ||");
        console.Output.WriteLine("========================================================================");
        console.Output.WriteLine("");

        return default;
    }
}