using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Domain.Discord;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class TokenCommandBase : ICommand
    {
        [CommandOption("token", 't', IsRequired = true, EnvironmentVariableName = "DISCORD_TOKEN", Description = "Authorization token.")]
        public string TokenValue { get; init; } = "";

        [CommandOption("bot", 'b', EnvironmentVariableName = "DISCORD_TOKEN_BOT", Description = "Authorize as a bot.")]
        public bool IsBotToken { get; init; }

        protected AuthToken GetAuthToken() => new(
            IsBotToken
                ? AuthTokenType.Bot
                : AuthTokenType.User,
            TokenValue
        );

        protected DiscordClient GetDiscordClient() => new(GetAuthToken());

        public abstract ValueTask ExecuteAsync(IConsole console);
    }
}