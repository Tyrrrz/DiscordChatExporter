using CommandLine;

namespace DiscordChatExporter.Cli.Options
{
    [Verb("dm", HelpText = "Get the list of direct message channels.")]
    public class GetDirectMessageChannelsOptions : AuthorizedOptions
    {
    }
}