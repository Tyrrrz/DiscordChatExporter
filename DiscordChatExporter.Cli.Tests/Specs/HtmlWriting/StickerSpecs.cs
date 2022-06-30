using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public class StickerSpecs : IClassFixture<ExportWrapperFixture>
{
    private readonly ExportWrapperFixture _exportWrapper;

    public StickerSpecs(ExportWrapperFixture exportWrapper)
    {
        _exportWrapper = exportWrapper;
    }

    [Fact]
    public async Task Message_with_a_PNG_based_sticker_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670623158943754")
        );

        // Assert
        var stickerUrl = message.QuerySelector("[title='rock'] img")?.GetAttribute("src");
        stickerUrl.Should().Be("https://discord.com/stickers/904215665597120572.png");
    }

    [Fact]
    public async Task Message_with_a_Lottie_based_sticker_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670526517997590")
        );

        // Assert
        var stickerUrl = message.QuerySelector("[title='Yikes'] [data-source]")?.GetAttribute("data-source");
        stickerUrl.Should().Be("https://discord.com/stickers/816087132447178774.json");
    }
}