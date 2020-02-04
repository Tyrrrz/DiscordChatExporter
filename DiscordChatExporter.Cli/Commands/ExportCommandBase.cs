using System;
using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Utilities;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    public abstract class ExportCommandBase : TokenCommandBase
    {
        protected SettingsService SettingsService { get; }

        protected ExportService ExportService { get; }

        [CommandOption("format", 'f', Description = "Output file format.")]
        public ExportFormat ExportFormat { get; set; } = ExportFormat.HtmlDark;

        [CommandOption("output", 'o', Description = "Output file or directory path.")]
        public string OutputPath { get; set; } = Directory.GetCurrentDirectory();

        [CommandOption("after", Description = "Limit to messages sent after this date.")]
        public DateTimeOffset? After { get; set; }

        [CommandOption("before", Description = "Limit to messages sent before this date.")]
        public DateTimeOffset? Before { get; set; }

        [CommandOption("partition", 'p', Description = "Split output into partitions limited to this number of messages.")]
        public int? PartitionLimit { get; set; }

        [CommandOption("dateformat", Description = "Date format used in output.")]
        public string? DateFormat { get; set; }

        protected ExportCommandBase(SettingsService settingsService, DataService dataService, ExportService exportService)
            : base(dataService)
        {
            SettingsService = settingsService;
            ExportService = exportService;
        }

        protected async ValueTask ExportAsync(IConsole console, Guild guild, Channel channel)
        {
            if (!string.IsNullOrWhiteSpace(DateFormat))
                SettingsService.DateFormat = DateFormat;

            console.Output.Write($"Exporting channel [{channel.Name}]... ");
            var progress = console.CreateProgressTicker();

            await ExportService.ExportChatLogAsync(Token, guild, channel,
                OutputPath, ExportFormat, PartitionLimit,
                After, Before, progress);

            console.Output.WriteLine();
        }

        protected async ValueTask ExportAsync(IConsole console, Channel channel)
        {
            var guild = await DataService.GetGuildAsync(Token, channel.GuildId);
            await ExportAsync(console, guild, channel);
        }

        protected async ValueTask ExportAsync(IConsole console, string channelId)
        {
            var channel = await DataService.GetChannelAsync(Token, channelId);
            await ExportAsync(console, channel);
        }
    }
}