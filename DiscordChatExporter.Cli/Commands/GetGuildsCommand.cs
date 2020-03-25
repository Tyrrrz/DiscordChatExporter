using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("guilds", Description = "Get the list of accessible guilds.")]
    public class GetGuildsCommand : TokenCommandBase
    {
        public GetGuildsCommand(DataService dataService)
            : base(dataService)
        {
        }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var guilds = await DataService.GetUserGuildsAsync(Token);

            foreach (var guild in guilds.OrderBy(g => g.Name))
                console.Output.WriteLine($"{guild.Id} | {guild.Name}");
        }
    }
}