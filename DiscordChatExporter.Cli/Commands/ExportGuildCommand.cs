using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportguild", Description = "Exports all channels within the specified server.")]
public class ExportGuildCommand : ExportCommandBase
{
    [CommandOption("guild", 'g', Description = "Server ID.")]
    public required Snowflake GuildId { get; init; }

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; init; } = true;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();
        var channels = new List<Channel>();

        await console.Output.WriteLineAsync("Fetching channels...");

        var fetchedChannelsCount = 0;
        await console
            .CreateStatusTicker()
            .StartAsync(
                "...",
                async ctx =>
                {
                    await foreach (
                        var channel in Discord.GetGuildChannelsAsync(GuildId, cancellationToken)
                    )
                    {
                        if (channel.IsCategory)
                            continue;

                        if (!IncludeVoiceChannels && channel.IsVoice)
                            continue;

                        channels.Add(channel);

                        ctx.Status(Markup.Escape($"Fetched '{channel.GetHierarchicalName()}'."));

                        fetchedChannelsCount++;
                    }
                }
            );

        await console.Output.WriteLineAsync($"Fetched {fetchedChannelsCount} channel(s).");

        await ExportAsync(console, channels);
    }
}
