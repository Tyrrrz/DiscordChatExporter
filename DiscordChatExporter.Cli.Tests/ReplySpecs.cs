using System.IO;
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
using Xunit;
using Xunit.Abstractions;

namespace DiscordChatExporter.Cli.Tests
{
    public class ReplySpecs : IClassFixture<TempOutputFixture>
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TempOutputFixture _tempOutput;

        public ReplySpecs(ITestOutputHelper testOutput, TempOutputFixture tempOutput)
        {
            _testOutput = testOutput;
            _tempOutput = tempOutput;
        }

        [Fact]
        public async Task Reply_to_a_normal_message_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = _tempOutput.GetTempFilePath("html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("reply-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.ReplyTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866460738239725598");
            var referenceHtml = messageHtml?.QuerySelector(".chatlog__reference-link");

            // Assert
            messageHtml.Should().NotBeNull();
            messageHtml?.Text().Trim().Should().Be("reply to original");
            referenceHtml?.Text().Trim().Should().Be("original");
        }

        [Fact]
        public async Task Reply_to_a_deleted_message_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = _tempOutput.GetTempFilePath("html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("reply-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.ReplyTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866460975388819486");
            var referenceHtml = messageHtml?.QuerySelector(".chatlog__reference-link");

            // Assert
            messageHtml.Should().NotBeNull();
            messageHtml?.Text().Trim().Should().Be("reply to deleted");
            referenceHtml?.Text().Trim().Should().Be("Original message was deleted or could not be loaded.");
        }

        [Fact]
        public async Task Reply_to_a_empty_message_with_attachment_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = _tempOutput.GetTempFilePath("html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("reply-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.ReplyTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866462470335627294");
            var referenceHtml = messageHtml?.QuerySelector(".chatlog__reference-link");

            // Assert
            messageHtml.Should().NotBeNull();
            messageHtml?.Text().Trim().Should().Be("reply to attachment");
            referenceHtml?.Text().Trim().Should().Be("Click to see attachment 🖼️");
        }
    }
}