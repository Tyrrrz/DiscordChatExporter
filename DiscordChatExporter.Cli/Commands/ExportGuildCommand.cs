using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportguild", Description = "Exports all channels within the specified guild.")]
public class ExportGuildCommand : ExportCommandBase
{
    [CommandOption(
        "guild",
        'g',
        Description = "Guild ID."
    )]
    public required Snowflake GuildId { get; init; }

    [CommandOption(
        "include-threads",
        Description = "Include threads in the export."
    )]

    public bool IncludeThreads { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        await console.Output.WriteLineAsync("Fetching channels...");

        var channels = (await Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
            .Where(c => c.Kind != ChannelKind.GuildCategory)
            .ToArray();

        var threads = Array.Empty<ChannelThread>();
        if (IncludeThreads)
        {
            threads = (await Discord.GetGuildThreadsAsync(GuildId, cancellationToken))
                .OrderBy(c => c.Name)
                .ToArray();
        }

        await base.ExecuteAsync(console, channels, threads);
    }
}