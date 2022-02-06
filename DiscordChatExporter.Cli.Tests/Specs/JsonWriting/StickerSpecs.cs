using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.JsonWriting;

public record StickerSpecs(ExportWrapperFixture ExportWrapper) : IClassFixture<ExportWrapperFixture>
{
    [Fact]
    public async Task Message_with_a_PNG_based_sticker_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670623158943754")
        );

        var sticker = message
            .GetProperty("stickers")
            .EnumerateArray()
            .Single();

        // Assert
        sticker.GetProperty("id").GetString().Should().Be("904215665597120572");
        sticker.GetProperty("name").GetString().Should().Be("rock");
        sticker.GetProperty("format").GetString().Should().Be("PngAnimated");
        sticker.GetProperty("sourceUrl").GetString().Should().Be("https://discord.com/stickers/904215665597120572.png");
    }

    [Fact]
    public async Task Message_with_a_Lottie_based_sticker_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670526517997590")
        );

        var sticker = message
            .GetProperty("stickers")
            .EnumerateArray()
            .Single();

        // Assert
        sticker.GetProperty("id").GetString().Should().Be("816087132447178774");
        sticker.GetProperty("name").GetString().Should().Be("Yikes");
        sticker.GetProperty("format").GetString().Should().Be("Lottie");
        sticker.GetProperty("sourceUrl").GetString().Should().Be("https://discord.com/stickers/816087132447178774.json");
    }
}