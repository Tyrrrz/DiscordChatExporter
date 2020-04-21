using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Domain.Discord;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class TokenCommandBase : ICommand
    {
        [CommandOption("token", 't', IsRequired = true, EnvironmentVariableName = "DISCORD_TOKEN",
            Description = "Authorization token.")]
        public string TokenValue { get; set; } = "";

        [CommandOption("bot", 'b', EnvironmentVariableName = "DISCORD_TOKEN_BOT",
            Description = "Whether this authorization token belongs to a bot.")]
        public bool IsBotToken { get; set; }

        protected AuthToken GetAuthToken() => new AuthToken(IsBotToken ? AuthTokenType.Bot : AuthTokenType.User, TokenValue);

        protected DiscordClient GetDiscordClient() => new DiscordClient(GetAuthToken());

        public abstract ValueTask ExecuteAsync(IConsole console);
    }
}