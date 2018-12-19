using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("export", HelpText = "Export channel chat log to a file.")]
    public class ExportChatOptions : ExportOptions
    {
        [Option('c', "channel", Required = true, HelpText = "Channel ID.")]
        public string ChannelId { get; set; }
    }
}