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
    public class EmbedSpecs : IClassFixture<TempOutputFixture>
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TempOutputFixture _tempOutput;

        public EmbedSpecs(ITestOutputHelper testOutput, TempOutputFixture tempOutput)
        {
            _testOutput = testOutput;
            _tempOutput = tempOutput;
        }

        [Fact]
        public async Task Message_with_an_embed_is_rendered_correctly_in_JSON()
        {
            // Arrange
            var outputFilePath = _tempOutput.GetTempFilePath("json");

            // Act
            var jsonData = await GlobalCache.WrapAsync("embed-specs-output-json", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.EmbedTestCases)},
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
                    "866769910729146400",
                    StringComparison.OrdinalIgnoreCase
                ));

            var embed = messageJson
                .GetProperty("embeds")
                .EnumerateArray()
                .Single();

            var embedAuthor = embed.GetProperty("author");
            var embedThumbnail = embed.GetProperty("thumbnail");
            var embedFooter = embed.GetProperty("footer");
            var embedFields = embed.GetProperty("fields").EnumerateArray().ToArray();

            // Assert
            embed.GetProperty("title").GetString().Should().Be("Embed title");
            embed.GetProperty("url").GetString().Should().Be("https://example.com");
            embed.GetProperty("timestamp").GetString().Should().Be("2021-07-14T21:00:00+00:00");
            embed.GetProperty("description").GetString().Should().Be("**Embed** _description_");
            embed.GetProperty("color").GetString().Should().Be("#58B9FF");
            embedAuthor.GetProperty("name").GetString().Should().Be("Embed author");
            embedAuthor.GetProperty("url").GetString().Should().Be("https://example.com/author");
            embedAuthor.GetProperty("iconUrl").GetString().Should().NotBeNullOrWhiteSpace();
            embedThumbnail.GetProperty("url").GetString().Should().NotBeNullOrWhiteSpace();
            embedThumbnail.GetProperty("width").GetInt32().Should().Be(120);
            embedThumbnail.GetProperty("height").GetInt32().Should().Be(120);
            embedFooter.GetProperty("text").GetString().Should().Be("Embed footer");
            embedFooter.GetProperty("iconUrl").GetString().Should().NotBeNullOrWhiteSpace();
            embedFields.Should().HaveCount(3);
            embedFields[0].GetProperty("name").GetString().Should().Be("Field 1");
            embedFields[0].GetProperty("value").GetString().Should().Be("Value 1");
            embedFields[0].GetProperty("isInline").GetBoolean().Should().BeTrue();
            embedFields[1].GetProperty("name").GetString().Should().Be("Field 2");
            embedFields[1].GetProperty("value").GetString().Should().Be("Value 2");
            embedFields[1].GetProperty("isInline").GetBoolean().Should().BeTrue();
            embedFields[2].GetProperty("name").GetString().Should().Be("Field 3");
            embedFields[2].GetProperty("value").GetString().Should().Be("Value 3");
            embedFields[2].GetProperty("isInline").GetBoolean().Should().BeTrue();
        }

        [Fact]
        public async Task Message_with_an_embed_is_rendered_correctly_in_HTML()
        {
            // Arrange
            var outputFilePath = _tempOutput.GetTempFilePath("html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("embed-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.EmbedTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866769910729146400");
            var messageText = messageHtml?.Text();

            // Assert
            messageText.Should().ContainAll(
                "Embed author",
                "Embed title",
                "Embed description",
                "Field 1", "Value 1",
                "Field 2", "Value 2",
                "Field 3", "Value 3",
                "Embed footer"
            );
        }

        [Fact]
        public async Task Message_with_a_Spotify_track_is_rendered_using_an_iframe_in_HTML()
        {
            // Arrange
            var outputFilePath = _tempOutput.GetTempFilePath("html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("embed-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.EmbedTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-867886632203976775");
            var iframeHtml = messageHtml?.QuerySelector("iframe");

            // Assert
            iframeHtml.Should().NotBeNull();
            iframeHtml?.GetAttribute("src").Should()
                .StartWithEquivalent("https://open.spotify.com/embed/track/1LHZMWefF9502NPfArRfvP");
        }

        [Fact]
        public async Task Message_with_a_YouTube_video_is_rendered_using_an_iframe_in_HTML()
        {
            // Arrange
            var outputFilePath = _tempOutput.GetTempFilePath("html");

            // Act
            var htmlData = await GlobalCache.WrapAsync("embed-specs-output-html", async () =>
            {
                await new ExportChannelsCommand
                {
                    TokenValue = Secrets.DiscordToken,
                    IsBotToken = Secrets.IsDiscordTokenBot,
                    ChannelIds = new[] {Snowflake.Parse(ChannelIds.EmbedTestCases)},
                    ExportFormat = ExportFormat.HtmlDark,
                    OutputPath = outputFilePath
                }.ExecuteAsync(new FakeConsole());

                return await File.ReadAllTextAsync(outputFilePath);
            });

            _testOutput.WriteLine(htmlData);

            var html = Html.Parse(htmlData);

            var messageHtml = html.QuerySelector("#message-866472508588294165");
            var iframeHtml = messageHtml?.QuerySelector("iframe");

            // Assert
            iframeHtml.Should().NotBeNull();
            iframeHtml?.GetAttribute("src").Should().StartWithEquivalent("https://www.youtube.com/embed/qOWW4OlgbvE");
        }
    }
}