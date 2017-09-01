using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Models;
using DiscordChatExporter.Services;
using Tyrrrz.Extensions;

namespace DiscordChatExporter
{
    public static class Program
    {
        private static readonly DiscordApiService DiscordApiService = new DiscordApiService();
        private static readonly HtmlExportService HtmlExportService = new HtmlExportService();

        private static Options GetOptions(string[] args)
        {
            // Parse the arguments
            var argsDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string arg in args)
            {
                var match = Regex.Match(arg, "/(.*?):\"?(.*?)\"?$");
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                if (key.IsBlank())
                    continue;

                argsDic[key] = value;
            }

            // Extract required arguments
            string token = argsDic.GetOrDefault("token");
            string channelId = argsDic.GetOrDefault("channelId");

            // Verify arguments
            if (token.IsBlank() || channelId.IsBlank())
                throw new ArgumentException("Some or all required command line arguments are missing");

            // Create option set
            return new Options(token, channelId);
        }

        private static async Task MainAsync(string[] args)
        {
            // Parse cmd args
            var options = GetOptions(args);

            // Get messages
            Console.WriteLine("Getting messages...");
            var messages = await DiscordApiService.GetMessagesAsync(options.Token, options.ChannelId);
            var chatLog = new ChatLog(options.ChannelId, messages);

            // Export
            Console.WriteLine("Exporting messages...");
            HtmlExportService.Export($"{options.ChannelId}.html", chatLog);
        }

        public static void Main(string[] args)
        {
            Console.Title = "Discord Chat Exporter";

            MainAsync(args).GetAwaiter().GetResult();
        }
    }
}