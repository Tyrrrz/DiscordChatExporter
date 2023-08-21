using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
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
        "include-vc",
        Description = "Include voice channels."
    )]
    public bool IncludeVoiceChannels { get; init; } = true;

    [CommandOption(
        "include-threads",
        Description = "Include threads."
    )]
    public bool IncludeThreads { get; init; } = false;

    [CommandOption(
        "include-archived-threads",
        Description = "Include archived threads."
    )]
    public bool IncludeArchivedThreads { get; init; } = false;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        // Cannot include archived threads without including active threads as well
        if (IncludeArchivedThreads && !IncludeThreads)
        {
            throw new CommandException(
                "Option --include-archived-threads can only be used when --include-threads is also specified."
            );
        }

        var cancellationToken = console.RegisterCancellationHandler();

        await console.Output.WriteLineAsync("Fetching channels...");

        var channels = (await Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
            .Where(c => c.Kind != ChannelKind.GuildCategory)
            .Where(c => IncludeVoiceChannels || !c.Kind.IsVoice())
            .ToArray();

        var threads = IncludeThreads
            ? await Discord.GetGuildThreadsAsync(GuildId, IncludeArchivedThreads, cancellationToken)
            : Array.Empty<Channel>();

        await ExportAsync(
            console,
            channels.Concat(threads).ToArray()
        );
    }
}