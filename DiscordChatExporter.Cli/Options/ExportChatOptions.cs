using System;
using CommandLine;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Cli.Options
{
    [Verb("export", HelpText = "Export channel chat log to a file.")]
    public class ExportChatOptions
    {
        [Option('t', "token", Required = true, HelpText = "Authorization token.")]
        public string TokenValue { get; set; }

        [Option('b', "bot", Default = false, HelpText = "Whether this authorization token belongs to a bot.")]
        public bool IsBotToken { get; set; }

        [Option('c', "channel", Required = true, HelpText = "Channel ID.")]
        public string ChannelId { get; set; }

        [Option('f', "format", Default = ExportFormat.HtmlDark, HelpText = "Output file format.")]
        public ExportFormat ExportFormat { get; set; }

        [Option('o', "output", Default = null, HelpText = "Output file path.")]
        public string FilePath { get; set; }

        [Option("from", Default = null, HelpText = "Limit to messages sent after this date.")]
        public DateTime? From { get; set; }

        [Option("to", Default = null, HelpText = "Limit to messages sent before this date.")]
        public DateTime? To { get; set; }

        [Option("dateformat", Default = null, HelpText = "Date format used in output.")]
        public string DateFormat { get; set; }

        [Option("grouplimit", Default = 0, HelpText = "Message group limit.")]
        public int MessageGroupLimit { get; set; }
    }
}