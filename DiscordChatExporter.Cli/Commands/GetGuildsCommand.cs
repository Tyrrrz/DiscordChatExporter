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
            // Get guilds
            var guilds = await DataService.GetUserGuildsAsync(Token);

            // Order guilds
            guilds = guilds.OrderBy(g => g.Name).ToArray();

            // Print result
            foreach (var guild in guilds)
                console.Output.WriteLine($"{guild.Id} | {guild.Name}");
        }
    }
}