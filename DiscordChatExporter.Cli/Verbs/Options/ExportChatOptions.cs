using System;
using CommandLine;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("export", HelpText = "Export channel chat log to a file.")]
    public class ExportChatOptions : TokenOptions
    {
        [Option('c', "channel", Required = true, HelpText = "Channel ID.")]
        public string ChannelId { get; set; }

        [Option('f', "format", Default = ExportFormat.HtmlDark, HelpText = "Output file format.")]
        public ExportFormat ExportFormat { get; set; }

        [Option('o', "output", Default = null, HelpText = "Output file path.")]
        public string FilePath { get; set; }

        [Option("after", Default = null, HelpText = "Limit to messages sent after this date.")]
        public DateTime? After { get; set; }

        [Option("before", Default = null, HelpText = "Limit to messages sent before this date.")]
        public DateTime? Before { get; set; }

        [Option('p', "partition", Default = null, HelpText = "Split output into partitions limited to this number of messages.")]
        public int? MaxMessageCountPerPartition { get; set; }

        [Option("dateformat", Default = null, HelpText = "Date format used in output.")]
        public string DateFormat { get; set; }

        [Option("grouplimit", Default = 0, HelpText = "Message group limit.")]
        public int MessageGroupLimit { get; set; }
    }
}