using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("dm", HelpText = "Get the list of direct message channels.")]
    public class GetDirectMessageChannelsOptions : TokenOptions
    {
    }
}