using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using JsonExtensions;
using Xunit;
using Xunit.Abstractions;

namespace DiscordChatExporter.Cli.Tests
{
    public class MentionSpecs : IClassFixture<TempOutputFixture>
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TempOutputFixture _tempOutput;

        public MentionSpecs(ITestOutputHelper testOutput, TempOutputFixture tempOutput)
        {
            _testOutput = testOutput;
            _tempOutput = tempOutput;
        }

        [Fact]
        public async Task User_mention_is_rendered_correctly_in_JSON()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "json");

            // Act
            var jsonData = await GlobalCache.WrapAsync("mention-specs-output-json", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.Json,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(jsonData);

            var json = Json.Parse(jsonData);

            var messageJson = json
                .GetProperty("messages")
                .EnumerateArray()
                .Single(j => string.Equals(
                    j.GetProperty("id").GetString(),
                    "866458840245076028",
                    StringComparison.OrdinalIgnoreCase
                ));

            var content = messageJson
                .GetProperty("content")
                .GetString();

            var mentionedUserIds = messageJson
                .GetProperty("mentions")
                .EnumerateArray()
                .Select(j => j.GetProperty("id").GetString())
                .ToArray();

            // Assert
            content.Should().Be("User mention: @Tyrrrz");
            mentionedUserIds.Should().Contain("128178626683338752");
        }

        [Fact]
        public async Task User_mention_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("mention-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866458840245076028");

            // Assert
            messageHtml.Should().NotBeNull();
            messageHtml?.Text().Trim().Should().Be("User mention: @Tyrrrz");
            messageHtml?.InnerHtml.Should().Contain("Tyrrrz#5447");
        }

        [Fact]
        public async Task Text_channel_mention_is_rendered_correctly_in_JSON()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "json");

            // Act
            var jsonData = await GlobalCache.WrapAsync("mention-specs-output-json", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.Json,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(jsonData);

            var json = Json.Parse(jsonData);

            var messageJson = json
                .GetProperty("messages")
                .EnumerateArray()
                .Single(j => string.Equals(
                    j.GetProperty("id").GetString(),
                    "866459040480624680",
                    StringComparison.OrdinalIgnoreCase
                ));

            var content = messageJson
                .GetProperty("content")
                .GetString();

            // Assert
            content.Should().Be("Text channel mention: #mention-tests");
        }

        [Fact]
        public async Task Text_channel_mention_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("mention-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866459040480624680");

            // Assert
            messageHtml.Should().NotBeNull();
            messageHtml?.Text().Trim().Should().Be("Text channel mention: #mention-tests");
        }

        [Fact]
        public async Task Voice_channel_mention_is_rendered_correctly_in_JSON()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "json");

            // Act
            var jsonData = await GlobalCache.WrapAsync("mention-specs-output-json", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.Json,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(jsonData);

            var json = Json.Parse(jsonData);

            var messageJson = json
                .GetProperty("messages")
                .EnumerateArray()
                .Single(j => string.Equals(
                    j.GetProperty("id").GetString(),
                    "866459175462633503",
                    StringComparison.OrdinalIgnoreCase
                ));

            var content = messageJson
                .GetProperty("content")
                .GetString();

            // Assert
            content.Should().Be("Voice channel mention: #chaos-vc [voice]");
        }

        [Fact]
        public async Task Voice_channel_mention_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("mention-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866459175462633503");

            // Assert
            messageHtml.Should().NotBeNull();
            messageHtml?.Text().Trim().Should().Be("Voice channel mention: 🔊chaos-vc");
        }

        [Fact]
        public async Task Role_mention_is_rendered_correctly_in_JSON()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "json");

            // Act
            var jsonData = await GlobalCache.WrapAsync("mention-specs-output-json", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.Json,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(jsonData);

            var json = Json.Parse(jsonData);

            var messageJson = json
                .GetProperty("messages")
                .EnumerateArray()
                .Single(j => string.Equals(
                    j.GetProperty("id").GetString(),
                    "866459254693429258",
                    StringComparison.OrdinalIgnoreCase
                ));

            var content = messageJson
                .GetProperty("content")
                .GetString();

            // Assert
            content.Should().Be("Role mention: @Role 1");
        }

        [Fact]
        public async Task Role_mention_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("mention-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.MentionTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866459254693429258");

            // Assert
            messageHtml.Should().NotBeNull();
            messageHtml?.Text().Trim().Should().Be("Role mention: @Role 1");
        }
    }
}