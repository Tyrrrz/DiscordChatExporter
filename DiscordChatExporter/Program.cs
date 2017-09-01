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
        private static readonly DiscordApiService ApiService = new DiscordApiService();
        private static readonly HtmlExportService ExportService = new HtmlExportService();

        private static Options GetOptions(string[] args)
        {
            // Parse the arguments
            var argsDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var arg in args)
            {
                var match = Regex.Match(arg, "/(.*?):\"?(.*?)\"?$");
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;

                if (key.IsBlank())
                    continue;

                argsDic[key] = value;
            }

            // Extract required arguments
            var token = argsDic.GetOrDefault("token");
            var channelId = argsDic.GetOrDefault("channelId");

            // Verify arguments
            if (token.IsBlank() || channelId.IsBlank())
                throw new ArgumentException("Some or all required command line arguments are missing");

            // Exract optional arguments
            var theme = argsDic.GetOrDefault("theme").ParseEnumOrDefault<Theme>();

            // Create option set
            return new Options(token, channelId, theme);
        }

        private static async Task MainAsync(string[] args)
        {
            // Parse cmd args
            var options = GetOptions(args);

            // Get messages
            Console.WriteLine("Getting messages...");
            var messages = await ApiService.GetMessagesAsync(options.Token, options.ChannelId);
            var chatLog = new ChatLog(options.ChannelId, messages);

            // Export
            Console.WriteLine("Exporting messages...");
            ExportService.Export($"{options.ChannelId}.html", chatLog, options.Theme);
        }

        public static void Main(string[] args)
        {
            Console.Title = "Discord Chat Exporter";

            MainAsync(args).GetAwaiter().GetResult();
        }
    }
}