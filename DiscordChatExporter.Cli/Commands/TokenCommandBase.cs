using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Services;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    public abstract class TokenCommandBase : ICommand
    {
        protected DataService DataService { get; }

        [CommandOption("token", 't', IsRequired = true, Description = "Authorization token.")]
        public string TokenValue { get; set; }

        [CommandOption("bot", 'b', Description = "Whether this authorization token belongs to a bot.")]
        public bool IsBotToken { get; set; }

        protected TokenCommandBase(DataService dataService)
        {
            DataService = dataService;
        }

        protected AuthToken GetToken() => new AuthToken(IsBotToken ? AuthTokenType.Bot : AuthTokenType.User, TokenValue);

        public abstract Task ExecuteAsync(IConsole console);
    }
}