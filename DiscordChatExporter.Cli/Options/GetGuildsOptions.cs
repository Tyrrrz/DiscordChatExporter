using CommandLine;

namespace DiscordChatExporter.Cli.Options
{
    [Verb("guilds", HelpText = "Get the list of accessible guilds.")]
    public class GetGuildsOptions : AuthorizedOptions
    {
    }
}