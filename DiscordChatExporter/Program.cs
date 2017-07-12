using System;
using System.Threading.Tasks;
using DiscordChatExporter.Models;
using DiscordChatExporter.Services;

namespace DiscordChatExporter
{
    public static class Program
    {
        private static readonly Options Options = new Options();

        private static readonly DiscordApiService DiscordApiService = new DiscordApiService();
        private static readonly ExportService ExportService = new ExportService();

        private static async Task MainAsync(string[] args)
        {
            // Parse cmd args
            CommandLine.Parser.Default.ParseArgumentsStrict(args, Options);

            // Get messages
            Console.WriteLine("Getting messages...");
            var messages = await DiscordApiService.GetMessagesAsync(Options.Token, Options.ChannelId);
            var chatLog = new ChatLog(Options.ChannelId, messages);

            // Export
            Console.WriteLine("Exporting messages...");
            ExportService.Export($"{Options.ChannelId}.html", chatLog);
        }

        public static void Main(string[] args)
        {
            Console.Title = "Discord Chat Exporter";

            MainAsync(args).GetAwaiter().GetResult();
        }
    }
}