using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportall", Description = "Export all accessible channels.")]
public class ExportAllCommand : ExportCommandBase
{
    [CommandOption(
        "include-dm",
        Description = "Include direct message channels."
    )]
    public bool IncludeDirectMessages { get; init; } = true;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();
        var channels = new List<Channel>();

        await console.Output.WriteLineAsync("Fetching channels...");
        await foreach (var guild in Discord.GetUserGuildsAsync(cancellationToken))
        {
            // Skip DMs if instructed to
            if (!IncludeDirectMessages && guild.Id == Guild.DirectMessages.Id)
                continue;

            await foreach (var channel in Discord.GetGuildChannelsAsync(guild.Id, cancellationToken))
                channels.Add(channel);
        }

        await base.ExecuteAsync(console, channels);
    }
}