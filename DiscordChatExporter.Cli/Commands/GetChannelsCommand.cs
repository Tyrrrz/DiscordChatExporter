using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Cli.Utils.Json;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("list channels", Description = "Gets the list of channels in one or more servers.")]
public partial class GetChannelsCommand : DiscordCommandBase
{
    [CommandParameter(0, Name = "server-ids", Description = "Server ID(s).")]
    public required IReadOnlyList<Snowflake> ServerIds { get; set; }

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

        var allChannels = new List<Channel>();

        foreach (var serverId in ServerIds)
        {
            var channels = (await Discord.GetGuildChannelsAsync(serverId, cancellationToken))
                .Where(c => !c.IsCategory)
                .Where(c => IncludeVoiceChannels || !c.IsVoice)
                .OrderBy(c => c.Parent?.Position)
                .ThenBy(c => c.Name)
                .ToArray();

            var threads =
                ThreadInclusionMode != ThreadInclusionMode.None
                    ? (
                        await Discord.GetGuildThreadsAsync(
                            serverId,
                            ThreadInclusionMode == ThreadInclusionMode.All,
                            null,
                            null,
                            cancellationToken
                        )
                    )
                        .OrderBy(c => c.Name)
                        .ToArray()
                    : [];

            foreach (var channel in channels)
            {
                allChannels.Add(channel);
                allChannels.AddRange(threads.Where(t => t.Parent?.Id == channel.Id));
            }
        }

        await console.Output.WriteLineAsync(
            JsonSerializer.Serialize(
                allChannels.ToArray(),
                CliJsonSerializerContext.Instance.ChannelArray
            )
        );
    }
}
