using System;
using System.Linq;
using CommandLine;
using DiscordChatExporter.Cli.Verbs;
using DiscordChatExporter.Cli.Verbs.Options;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static void PrintTokenHelp()
        {
            Console.WriteLine("# To get user token:");
            Console.WriteLine(" 1. Open Discord app");
            Console.WriteLine(" 2. Log in if you haven't");
            Console.WriteLine(" 3. Press Ctrl+Shift+I to show developer tools");
            Console.WriteLine(" 4. Press Ctrl+R to trigger reload");
            Console.WriteLine(" 5. Navigate to the Application tab");
            Console.WriteLine(" 6. Select \"Local Storage\" > \"https://discordapp.com\" on the left");
            Console.WriteLine(" 7. Find \"token\" under key and copy the value");
            Console.WriteLine();
            Console.WriteLine("# To get bot token:");
            Console.WriteLine(" 1. Go to Discord developer portal");
            Console.WriteLine(" 2. Log in if you haven't");
            Console.WriteLine(" 3. Open your application's settings");
            Console.WriteLine(" 4. Navigate to the Bot section on the left");
            Console.WriteLine(" 5. Under Token click Copy");
            Console.WriteLine();
            Console.WriteLine("# To get guild or channel ID:");
            Console.WriteLine(" 1. Open Discord app");
            Console.WriteLine(" 2. Log in if you haven't");
            Console.WriteLine(" 3. Open Settings");
            Console.WriteLine(" 4. Go to Appearance section");
            Console.WriteLine(" 5. Enable Developer Mode");
            Console.WriteLine(" 6. Right click on the desired guild or channel and click Copy ID");
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

            // Show token help if help requested or no verb specified
            parsedArgs.WithNotParsed(errs =>
            {
                var err = errs.First();

                if (err.Tag == ErrorType.NoVerbSelectedError)
                    PrintTokenHelp();

                if (err.Tag == ErrorType.HelpVerbRequestedError && args.Length == 1)
                    PrintTokenHelp();
            });
        }
    }
}