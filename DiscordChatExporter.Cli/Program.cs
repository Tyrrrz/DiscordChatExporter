using CommandLine;
using DiscordChatExporter.Cli.Verbs;
using DiscordChatExporter.Cli.Verbs.Options;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Get all verb types
            var verbTypes = new[]
            {
                typeof(ExportChatOptions),
                typeof(GetChannelsOptions),
                typeof(GetDirectMessageChannelsOptions),
                typeof(GetGuildsOptions),
                typeof(UpdateAppOptions)
            };

            // Parse command line arguments
            var parsedArgs = Parser.Default.ParseArguments(args, verbTypes);

            // Execute commands
            parsedArgs.WithParsed<ExportChatOptions>(o => new ExportChatVerb(o).Execute());
            parsedArgs.WithParsed<GetChannelsOptions>(o => new GetChannelsVerb(o).Execute());
            parsedArgs.WithParsed<GetDirectMessageChannelsOptions>(o => new GetDirectMessageChannelsVerb(o).Execute());
            parsedArgs.WithParsed<GetGuildsOptions>(o => new GetGuildsVerb(o).Execute());
            parsedArgs.WithParsed<UpdateAppOptions>(o => new UpdateAppVerb(o).Execute());
        }
    }
}