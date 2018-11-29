using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Verbs.Options;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli.Verbs
{
    public class GetChannelsVerb : Verb<GetChannelsOptions>
    {
        public GetChannelsVerb(GetChannelsOptions options)
            : base(options)
        {
        }

        public override async Task ExecuteAsync()
        {
            // Get data service
            var dataService = Container.Instance.Get<DataService>();

            // Get channels
            var channels = await dataService.GetGuildChannelsAsync(Options.GetToken(), Options.GuildId);

            // Print result
            foreach (var channel in channels.Where(c => c.Type.IsEither(ChannelType.GuildTextChat))
                .OrderBy(c => c.Name))
                Console.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}