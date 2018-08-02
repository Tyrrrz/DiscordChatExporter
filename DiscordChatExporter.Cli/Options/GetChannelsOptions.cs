using CommandLine;

namespace DiscordChatExporter.Cli.Options
{
    [Verb("channels", HelpText = "Get the list of channels in the given guild.")]
    public class GetChannelsOptions : AuthorizedOptions
    {
        [Option('g', "guild", Required = true, HelpText = "Guild ID.")]
        public string GuildId { get; set; }
    }
}