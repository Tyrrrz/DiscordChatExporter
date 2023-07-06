﻿using System;
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
        "include-vc",
        Description = "Include voice channels."
    )]
    public bool IncludeVoiceChannels { get; init; } = true;


    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        await console.Output.WriteLineAsync("Fetching channels...");

        var channels = (await Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
            .Where(c => c.Kind != ChannelKind.GuildCategory)
            .Where(c => IncludeVoiceChannels || !c.Kind.IsVoice())
            .ToArray();

        await base.ExecuteAsync(console, channels);
    }
}