using System;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Cli
{
    public class CliOptions
    {
        public string Token { get; set; }

        public string ChannelId { get; set; }

        public ExportFormat ExportFormat { get; set; }

        public string FilePath { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }
}