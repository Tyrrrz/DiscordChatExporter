using System;
using CommandLine;
using DiscordChatExporter.Cli.Verbs;
using DiscordChatExporter.Cli.Verbs.Options;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static void ShowTokenHelp()
        {
            Console.WriteLine("# To get user token:");
            Console.WriteLine(" - Open Discord app");
            Console.WriteLine(" - Log in if you haven't");
            Console.WriteLine(" - Press Ctrl+Shift+I to show developer tools");
            Console.WriteLine(" - Navigate to the Application tab");
            Console.WriteLine(" - Expand Storage > Local Storage > https://discordapp.com");
            Console.WriteLine(" - Find the \"token\" key and copy its value");
            Console.WriteLine();
            Console.WriteLine("# To get bot token:");
            Console.WriteLine(" - Go to Discord developer portal");
            Console.WriteLine(" - Log in if you haven't");
            Console.WriteLine(" - Open your application's settings");
            Console.WriteLine(" - Navigate to the Bot section on the left");
            Console.WriteLine(" - Under Token click Copy");
        }

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

            // Show token help if error
            if (parsedArgs.Tag == ParserResultType.NotParsed)
                ShowTokenHelp();
        }
    }
}