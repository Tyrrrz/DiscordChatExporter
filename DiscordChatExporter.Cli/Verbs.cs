using System;
using System.IO;
using System.Linq;
using DiscordChatExporter.Cli.Options;
using DiscordChatExporter.Core.Models;
using PowerArgs;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Cli
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class Verbs
    {
        private readonly Container _container = new Container();

        [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
        public bool Help { get; set; }

        private AuthToken CreateToken(bool isBotToken, string tokenValue) =>
            new AuthToken(isBotToken ? AuthTokenType.Bot : AuthTokenType.User, tokenValue);

        [ArgActionMethod, ArgDescription("Gets the list of all accessible guilds")]
        public void GetGuilds(GetGuildsOptions options)
        {
            // Create token
            var token = CreateToken(options.IsBotToken, options.TokenValue);

            // Get guilds
            var guilds = _container.DataService.GetUserGuildsAsync(token)
                .GetAwaiter().GetResult();

            // Print
            foreach (var guild in guilds.OrderBy(g => g.Name))
                Console.WriteLine($"{guild.Id} | {guild.Name}");
        }

        [ArgActionMethod, ArgDescription("Gets the list of all channels in the given guild")]
        public void GetChannels(GetChannelsOptions options)
        {
            // Create token
            var token = CreateToken(options.IsBotToken, options.TokenValue);

            // Get guilds
            var channels = _container.DataService.GetGuildChannelsAsync(token, options.GuildId)
                .GetAwaiter().GetResult();

            // Print
            foreach (var channel in channels.OrderBy(c => c.Name))
                Console.WriteLine($"{channel.Id} | {channel.Name}");
        }

        [ArgActionMethod, ArgDescription("Exports chat log to a file")]
        public void ExportChat(ExportChatOptions options)
        {
            // Configure settings
            if (options.DateFormat.IsNotBlank())
                _container.SettingsService.DateFormat = options.DateFormat;
            if (options.MessageGroupLimit >= 0)
                _container.SettingsService.MessageGroupLimit = options.MessageGroupLimit;

            // Create token
            var token = CreateToken(options.IsBotToken, options.TokenValue);

            // Get channel and guild
            var channel = _container.DataService.GetChannelAsync(token, options.ChannelId).GetAwaiter().GetResult();
            var guild = channel.GuildId == Guild.DirectMessages.Id
                ? Guild.DirectMessages
                : _container.DataService.GetGuildAsync(token, channel.GuildId).GetAwaiter().GetResult();

            // Generate file path if not set
            var filePath = options.FilePath;
            if (filePath.IsBlank())
            {
                filePath = $"{guild.Name} - {channel.Name}.{options.ExportFormat.GetFileExtension()}"
                    .Replace(Path.GetInvalidFileNameChars(), '_');
            }

            // TODO: extract this to make it reusable across implementations
            // Get messages
            var messages = _container.DataService
                .GetChannelMessagesAsync(token, channel.Id, options.From, options.To)
                .GetAwaiter().GetResult();

            // Group messages
            var messageGroups = _container.MessageGroupService.GroupMessages(messages);

            // Get mentionables
            var mentionables = _container.DataService.GetMentionablesAsync(token, guild.Id, messages)
                .GetAwaiter().GetResult();

            // Create log
            var log = new ChatLog(guild, channel, options.From, options.To, messageGroups, mentionables);

            // Export
            _container.ExportService.Export(options.ExportFormat, filePath, log);
        }

        [ArgActionMethod, ArgShortcut("update"), ArgDescription("Updates this command line app to the latest version")]
        public void UpdateApp(UpdateAppOptions options)
        {
            // TODO: this is configured only for GUI
            // Get update version
            var updateVersion = _container.UpdateService.CheckPrepareUpdateAsync().GetAwaiter().GetResult();

            if (updateVersion != null)
            {
                Console.WriteLine($"Updating to version {updateVersion}");

                _container.UpdateService.NeedRestart = false;
                _container.UpdateService.FinalizeUpdate();
            }
            else
            {
                Console.WriteLine("There are no application updates available.");
            }
        }
    }
}