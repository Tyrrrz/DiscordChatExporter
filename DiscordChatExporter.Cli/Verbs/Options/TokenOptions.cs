using CommandLine;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    public class TokenOptions
    {
        [Option('t', "token", Required = true, HelpText = "Authorization token.")]
        public string TokenValue { get; set; }

        [Option('b', "bot", Default = false, HelpText = "Whether this authorization token belongs to a bot.")]
        public bool IsBotToken { get; set; }

        public AuthToken GetToken() => new AuthToken(IsBotToken ? AuthTokenType.Bot : AuthTokenType.User, TokenValue);
    }
}