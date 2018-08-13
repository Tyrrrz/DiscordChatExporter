using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Verbs
{
    public class GetGuildsVerb : Verb<GetGuildsOptions>
    {
        public GetGuildsVerb(GetGuildsOptions options) 
            : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // Get data service
            var container = new Container();
            var dataService = container.Resolve<IDataService>();

            // Get guilds
            var guilds = await dataService.GetUserGuildsAsync(Options.GetToken());

            // Print result
            foreach (var guild in guilds.OrderBy(g => g.Name))
                Console.WriteLine($"{guild.Id} | {guild.Name}");
        }
    }
}