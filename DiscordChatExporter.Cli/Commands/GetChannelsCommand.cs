using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("list channels", Description = "Gets the list of channels in one or more servers.")]
public partial class GetChannelsCommand : DiscordCommandBase
{
    [CommandParameter(0, Name = "guild-ids", Description = "Server ID(s).")]
    public required IReadOnlyList<Snowflake> GuildIds { get; set; }

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; set; } = true;

    [CommandOption(
        "include-threads",
        Description = "Which types of threads should be included.",
        Converter = typeof(ThreadInclusionModeInputConverter)
    )]
    public ThreadInclusionMode ThreadInclusionMode { get; set; } = ThreadInclusionMode.None;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        foreach (var guildId in GuildIds)
        {
            var channels = (await Discord.GetGuildChannelsAsync(guildId, cancellationToken))
                .Where(c => !c.IsCategory)
                .Where(c => IncludeVoiceChannels || !c.IsVoice)
                .OrderBy(c => c.Parent?.Position)
                .ThenBy(c => c.Name)
                .ToArray();

            var threads =
                ThreadInclusionMode != ThreadInclusionMode.None
                    ? (
                        await Discord.GetGuildThreadsAsync(
                            guildId,
                            ThreadInclusionMode == ThreadInclusionMode.All,
                            null,
                            null,
                            cancellationToken
                        )
                    )
                        .OrderBy(c => c.Name)
                        .ToArray()
                    : [];

            // If output is redirected, print only channel IDs (one per line) for easy piping
            if (console.IsOutputRedirected)
            {
                foreach (var channel in channels)
                {
                    await console.Output.WriteLineAsync(channel.Id.ToString());
                    foreach (var channelThread in threads.Where(t => t.Parent?.Id == channel.Id))
                        await console.Output.WriteLineAsync(channelThread.Id.ToString());
                }
            }
            else
            {
                // Show guild header when listing multiple guilds
                if (GuildIds.Count > 1)
                {
                    var guild = await Discord.GetGuildAsync(guildId, cancellationToken);

                    using (console.WithForegroundColor(ConsoleColor.Cyan))
                        await console.Output.WriteLineAsync($"{guild.Id} | {guild.Name}");
                }

                var channelIdMaxLength = channels
                    .Select(c => c.Id.ToString().Length)
                    .OrderDescending()
                    .FirstOrDefault();

                foreach (var channel in channels)
                {
                    // Channel ID
                    await console.Output.WriteAsync(
                        channel.Id.ToString().PadRight(channelIdMaxLength, ' ')
                    );

                    // Separator
                    using (console.WithForegroundColor(ConsoleColor.DarkGray))
                        await console.Output.WriteAsync(" | ");

                    // Channel name
                    using (console.WithForegroundColor(ConsoleColor.White))
                        await console.Output.WriteLineAsync(channel.GetHierarchicalName());

                    var channelThreads = threads.Where(t => t.Parent?.Id == channel.Id).ToArray();
                    var channelThreadIdMaxLength = channelThreads
                        .Select(t => t.Id.ToString().Length)
                        .OrderDescending()
                        .FirstOrDefault();

                    foreach (var channelThread in channelThreads)
                    {
                        // Indent
                        await console.Output.WriteAsync(" * ");

                        // Thread ID
                        await console.Output.WriteAsync(
                            channelThread.Id.ToString().PadRight(channelThreadIdMaxLength, ' ')
                        );

                        // Separator
                        using (console.WithForegroundColor(ConsoleColor.DarkGray))
                            await console.Output.WriteAsync(" | ");

                        // Thread name
                        using (console.WithForegroundColor(ConsoleColor.White))
                            await console.Output.WriteAsync($"Thread / {channelThread.Name}");

                        // Separator
                        using (console.WithForegroundColor(ConsoleColor.DarkGray))
                            await console.Output.WriteAsync(" | ");

                        // Thread status
                        using (console.WithForegroundColor(ConsoleColor.White))
                            await console.Output.WriteLineAsync(
                                channelThread.IsArchived ? "Archived" : "Active"
                            );
                    }
                }

                if (GuildIds.Count > 1)
                    await console.Output.WriteLineAsync();
            }
        }
    }
}
