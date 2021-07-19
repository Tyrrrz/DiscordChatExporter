using System.IO;
using System.Threading.Tasks;
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
        public async Task Message_with_YouTube_video_is_rendered_with_a_player_in_HTML()
        {
            // Arrange
            var outputFilePath = Path.ChangeExtension(_tempOutput.GetTempFilePath(), "html");

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
            var iframeSrc = iframeHtml?.GetAttribute("src");

            // Assert
            iframeHtml.Should().NotBeNull();
            iframeSrc.Should().StartWithEquivalent("https://www.youtube.com/embed/qOWW4OlgbvE");
        }
    }
}