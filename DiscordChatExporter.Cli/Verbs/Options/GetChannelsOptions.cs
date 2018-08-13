using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("channels", HelpText = "Get the list of channels in the given guild.")]
    public class GetChannelsOptions : TokenOptions
    {
        [Option('g', "guild", Required = true, HelpText = "Guild ID.")]
        public string GuildId { get; set; }
    }
}