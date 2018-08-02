using CommandLine;

namespace DiscordChatExporter.Cli.Options
{
    [Verb("guilds", HelpText = "Get the list of accessible guilds.")]
    public class GetGuildsOptions
    {
        [Option('t', "token", Required = true, HelpText = "Authorization token.")]
        public string TokenValue { get; set; }

        [Option('b', "bot", Default = false, HelpText = "Whether this authorization token belongs to a bot.")]
        public bool IsBotToken { get; set; }
    }
}