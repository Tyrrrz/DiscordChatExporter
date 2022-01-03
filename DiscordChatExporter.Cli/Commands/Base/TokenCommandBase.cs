using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Cli.Commands.Base;

public abstract class TokenCommandBase : ICommand
{
    [CommandOption("token", 't', IsRequired = true, EnvironmentVariable = "DISCORD_TOKEN", Description = "Authentication token.")]
    public string Token { get; init; } = "";

    [CommandOption("bot", 'b', EnvironmentVariable = "DISCORD_TOKEN_BOT", Description = "This option doesn't do anything. Kept for backwards compatibility.")]
    [Obsolete("This option doesn't do anything. Kept for backwards compatibility.")]
    public bool IsBotToken { get; init; }

    private DiscordClient? _discordClient;
    protected DiscordClient Discord => _discordClient ??= new DiscordClient(Token);

    public abstract ValueTask ExecuteAsync(IConsole console);
}