using CommandLine;

namespace DiscordChatExporter.Cli.Options
{
    [Verb("channels", HelpText = "Get the list of channels in the given guild.")]
    public class GetChannelsOptions
    {
        [Option('t', "token", Required = true, HelpText = "Authorization token.")]
        public string TokenValue { get; set; }

        [Option('b', "bot", Default = false, HelpText = "Whether this authorization token belongs to a bot.")]
        public bool IsBotToken { get; set; }

        [Option('g', "guild", Required = true, HelpText = "Guild ID.")]
        public string GuildId { get; set; }
    }
}