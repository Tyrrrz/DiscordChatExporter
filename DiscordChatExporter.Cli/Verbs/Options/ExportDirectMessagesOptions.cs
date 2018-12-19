using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("exportdm", HelpText = "Export all direct message channels.")]
    public class ExportDirectMessagesOptions : ExportOptions
    {
    }
}