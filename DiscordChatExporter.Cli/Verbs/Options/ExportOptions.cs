using System;
using CommandLine;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    public abstract class ExportOptions : TokenOptions
    {
        [Option('f', "format", Default = ExportFormat.HtmlDark, HelpText = "Output file format.")]
        public ExportFormat ExportFormat { get; set; }

        [Option('o', "output", Default = null, HelpText = "Output file or directory path.")]
        public string OutputPath { get; set; }

        // HACK: CommandLineParser doesn't support DateTimeOffset
        [Option("after", Default = null, HelpText = "Limit to messages sent after this date.")]
        public DateTime? After { get; set; }

        // HACK: CommandLineParser doesn't support DateTimeOffset
        [Option("before", Default = null, HelpText = "Limit to messages sent before this date.")]
        public DateTime? Before { get; set; }

        [Option('p', "partition", Default = null, HelpText = "Split output into partitions limited to this number of messages.")]
        public int? PartitionLimit { get; set; }

        [Option("dateformat", Default = null, HelpText = "Date format used in output.")]
        public string DateFormat { get; set; }
    }
}