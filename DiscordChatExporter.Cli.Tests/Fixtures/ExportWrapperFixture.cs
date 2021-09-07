using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using JsonExtensions;

namespace DiscordChatExporter.Cli.Tests.Fixtures
{
    public class ExportWrapperFixture : IDisposable
    {
        private string DirPath { get; } = Path.Combine(
            Path.GetDirectoryName(typeof(ExportWrapperFixture).Assembly.Location) ?? Directory.GetCurrentDirectory(),
            "ExportCache",
            Guid.NewGuid().ToString()
        );

        public async ValueTask<IHtmlDocument> ExportAsHtmlAsync(Snowflake channelId)
        {
            var filePath = Path.Combine(DirPath, channelId + ".html");

            // Perform export only if it hasn't been done before
            if (!File.Exists(filePath))
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] { channelId },
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = filePath
                }.ExecuteAsync(new FakeConsole());
            }

            var data = await File.ReadAllTextAsync(filePath);

            return Html.Parse(data);
        }

        public async ValueTask<IElement> GetMessageAsHtmlAsync(Snowflake channelId, Snowflake messageId)
        {
            var document = await ExportAsHtmlAsync(channelId);

            var message = document.QuerySelector("#message-" + messageId);

            if (message is null)
            {
                throw new InvalidOperationException(
                    $"Message '{messageId}' does not exist in export of channel '{channelId}'."
                );
            }

            return message;
        }

        public async ValueTask<JsonElement> ExportAsJsonAsync(Snowflake channelId)
        {
            var filePath = Path.Combine(DirPath, channelId + ".json");

            // Perform export only if it hasn't been done before
            if (!File.Exists(filePath))
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] { channelId },
                    ExportFormat = ExportFormat.Json,
                    OutputPath = filePath
                }.ExecuteAsync(new FakeConsole());
            }

            var data = await File.ReadAllTextAsync(filePath);

            return Json.Parse(data);
        }

        public async ValueTask<JsonElement> GetMessageAsJsonAsync(Snowflake channelId, Snowflake messageId)
        {
            var document = await ExportAsJsonAsync(channelId);

            var message = document
                .GetProperty("messages")
                .EnumerateArray()
                .SingleOrDefault(j => string.Equals(
                    j.GetProperty("id").GetString(),
                    messageId.ToString(),
                    StringComparison.OrdinalIgnoreCase
                ));

            if (message.ValueKind == JsonValueKind.Undefined)
            {
                throw new InvalidOperationException(
                    $"Message '{messageId}' does not exist in export of channel '{channelId}'."
                );
            }

            return message;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(DirPath, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }
    }
}