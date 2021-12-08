using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Cli.Commands.Base;

public abstract class TokenCommandBase : ICommand
{
    [CommandOption("token", 't', IsRequired = true, EnvironmentVariable = "DISCORD_TOKEN", Description = "Authentication token.")]
    public string TokenValue { get; init; } = "";

    [CommandOption("bot", 'b', EnvironmentVariable = "DISCORD_TOKEN_BOT", Description = "Authenticate as a bot.")]
    public bool IsBotToken { get; init; }

    private AuthToken? _authToken;
    private AuthToken AuthToken => _authToken ??= new AuthToken(
        IsBotToken
            ? AuthTokenKind.Bot
            : AuthTokenKind.User,
        TokenValue
    );

    private DiscordClient? _discordClient;
    protected DiscordClient Discord => _discordClient ??= new DiscordClient(AuthToken);

    public abstract ValueTask ExecuteAsync(IConsole console);
}