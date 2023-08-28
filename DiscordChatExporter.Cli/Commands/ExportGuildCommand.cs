using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportguild", Description = "Exports all channels within the specified guild.")]
public class ExportGuildCommand : ExportCommandBase
{
    [CommandOption("guild", 'g', Description = "Guild ID.")]
    public required Snowflake GuildId { get; init; }

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; init; } = true;

    [CommandOption(
        "include-threads",
        Description = "Which types of threads should be included.",
        Converter = typeof(ThreadInclusionBindingConverter)
    )]
    public ThreadInclusion ThreadInclusion { get; init; } = ThreadInclusion.None;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();
        var channels = new List<Channel>();

        await console.Output.WriteLineAsync("Fetching channels...");

        // Regular channels
        await foreach (var channel in Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
        {
            if (channel.Kind == ChannelKind.GuildCategory)
                continue;

            if (!IncludeVoiceChannels && channel.Kind.IsVoice())
                continue;

            channels.Add(channel);
        }

        // Threads
        if (ThreadInclusion != ThreadInclusion.None)
        {
            await foreach (
                var thread in Discord.GetGuildThreadsAsync(
                    GuildId,
                    ThreadInclusion == ThreadInclusion.All,
                    cancellationToken
                )
            )
            {
                channels.Add(thread);
            }
        }

        await ExportAsync(console, channels);
    }
}
