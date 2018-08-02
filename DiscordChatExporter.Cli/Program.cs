using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using DiscordChatExporter.Cli.Options;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli
{
    public static class Program
    {
        private static readonly Container Container = new Container();

        private static IEnumerable<Type> GetVerbTypes()
        {
            yield return typeof(ExportChatOptions);
            yield return typeof(GetChannelsOptions);
            yield return typeof(GetGuildsOptions);
            yield return typeof(UpdateAppOptions);
        }

        private static AuthToken CreateToken(bool isBotToken, string tokenValue) =>
            new AuthToken(isBotToken ? AuthTokenType.Bot : AuthTokenType.User, tokenValue);

        private static void ExportChat(ExportChatOptions options)
        {
            // Configure settings
            if (options.DateFormat.IsNotBlank())
                Container.SettingsService.DateFormat = options.DateFormat;
            if (options.MessageGroupLimit > 0)
                Container.SettingsService.MessageGroupLimit = options.MessageGroupLimit;

            // Create token
            var token = CreateToken(options.IsBotToken, options.TokenValue);

            // Get channel and guild
            var channel = Container.DataService.GetChannelAsync(token, options.ChannelId).GetAwaiter().GetResult();
            var guild = channel.GuildId == Guild.DirectMessages.Id
                ? Guild.DirectMessages
                : Container.DataService.GetGuildAsync(token, channel.GuildId).GetAwaiter().GetResult();

            // Generate file path if not set
            var filePath = options.FilePath;
            if (filePath.IsBlank())
            {
                filePath = $"{guild.Name} - {channel.Name}.{options.ExportFormat.GetFileExtension()}"
                    .Replace(Path.GetInvalidFileNameChars(), '_');
            }

            // TODO: extract this to make it reusable across implementations
            // Get messages
            var messages = Container.DataService
                .GetChannelMessagesAsync(token, channel.Id, options.From, options.To)
                .GetAwaiter().GetResult();

            // Group messages
            var messageGroups = Container.MessageGroupService.GroupMessages(messages);

            // Get mentionables
            var mentionables = Container.DataService.GetMentionablesAsync(token, guild.Id, messages)
                .GetAwaiter().GetResult();

            // Create log
            var log = new ChatLog(guild, channel, options.From, options.To, messageGroups, mentionables);

            // Export
            Container.ExportService.Export(options.ExportFormat, filePath, log);

            // Print result
            Console.WriteLine($"Exported chat to [{filePath}]");
        }

        private static void GetChannels(GetChannelsOptions options)
        {
            // Create token
            var token = CreateToken(options.IsBotToken, options.TokenValue);

            // Get guilds
            var channels = Container.DataService.GetGuildChannelsAsync(token, options.GuildId)
                .GetAwaiter().GetResult();

            // Print result
            foreach (var channel in channels.OrderBy(c => c.Name))
                Console.WriteLine($"{channel.Id} | {channel.Name}");
        }

        private static void GetGuilds(GetGuildsOptions options)
        {
            // Create token
            var token = CreateToken(options.IsBotToken, options.TokenValue);

            // Get guilds
            var guilds = Container.DataService.GetUserGuildsAsync(token)
                .GetAwaiter().GetResult();

            // Print result
            foreach (var guild in guilds.OrderBy(g => g.Name))
                Console.WriteLine($"{guild.Id} | {guild.Name}");
        }

        private static void UpdateApp(UpdateAppOptions options)
        {
            // TODO: this is configured only for GUI
            // Get update version
            var updateVersion = Container.UpdateService.CheckPrepareUpdateAsync().GetAwaiter().GetResult();

            if (updateVersion != null)
            {
                Console.WriteLine($"Updating to version {updateVersion}");

                Container.UpdateService.NeedRestart = false;
                Container.UpdateService.FinalizeUpdate();
            }
            else
            {
                Console.WriteLine("There are no application updates available.");
            }
        }

        public static void Main(string[] args)
        {
            var parsedArgs = Parser.Default.ParseArguments(args, GetVerbTypes().ToArray());

            parsedArgs.WithParsed<ExportChatOptions>(ExportChat);
            parsedArgs.WithParsed<GetChannelsOptions>(GetChannels);
            parsedArgs.WithParsed<GetGuildsOptions>(GetGuilds);
            parsedArgs.WithParsed<UpdateAppOptions>(UpdateApp);
        }
    }
}