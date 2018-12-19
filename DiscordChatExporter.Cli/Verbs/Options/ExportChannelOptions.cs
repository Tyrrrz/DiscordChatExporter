using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("export", HelpText = "Export channel.")]
    public class ExportChannelOptions : ExportOptions
    {
        [Option('c', "channel", Required = true, HelpText = "Channel ID.")]
        public string ChannelId { get; set; }
    }
}