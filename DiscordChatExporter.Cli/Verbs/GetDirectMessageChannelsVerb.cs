using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Verbs
{
    public class GetDirectMessageChannelsVerb : Verb<GetDirectMessageChannelsOptions>
    {
        public GetDirectMessageChannelsVerb(GetDirectMessageChannelsOptions options)
            : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // Get data service
            var dataService = Container.Instance.Get<DataService>();

            // Get channels
            var channels = await dataService.GetDirectMessageChannelsAsync(Options.GetToken());

            // Order channels
            channels = channels.OrderBy(c => c.Name).ToArray();

            // Print result
            foreach (var channel in channels)
                Console.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}