using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public record StickerSpecs(ExportWrapperFixture ExportWrapper) : IClassFixture<ExportWrapperFixture>
{
    [Fact]
    public async Task Message_with_a_PNG_based_sticker_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670623158943754")
        );

        var container = message.QuerySelector("[title='rock']");
        var sourceUrl = container?.QuerySelector("img")?.GetAttribute("src");

        // Assert
        container.Should().NotBeNull();
        sourceUrl.Should().Be("https://discord.com/stickers/904215665597120572.png");
    }

    [Fact]
    public async Task Message_with_a_Lottie_based_sticker_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670526517997590")
        );

        var container = message.QuerySelector("[title='Yikes']");
        var sourceUrl = container?.QuerySelector("div[data-source]")?.GetAttribute("data-source");

        // Assert
        container.Should().NotBeNull();
        sourceUrl.Should().Be("https://discord.com/stickers/816087132447178774.json");
    }
}