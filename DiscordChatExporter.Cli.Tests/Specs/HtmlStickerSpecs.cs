using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlStickerSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_PNG_sticker()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670623158943754")
        );

        // Assert
        var stickerUrl = message.QuerySelector("[title='rock'] img")?.GetAttribute("src");
        stickerUrl.Should().StartWith("https://cdn.discordapp.com/stickers/904215665597120572.png");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_Lottie_sticker()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670526517997590")
        );

        // Assert
        var stickerUrl = message
            .QuerySelector("[title='Yikes'] [data-source]")
            ?.GetAttribute("data-source");

        stickerUrl
            .Should()
            .StartWith("https://cdn.discordapp.com/stickers/816087132447178774.json");
    }
}
