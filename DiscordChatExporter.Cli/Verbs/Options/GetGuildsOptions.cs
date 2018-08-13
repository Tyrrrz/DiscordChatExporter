using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("guilds", HelpText = "Get the list of accessible guilds.")]
    public class GetGuildsOptions : TokenOptions
    {
    }
}