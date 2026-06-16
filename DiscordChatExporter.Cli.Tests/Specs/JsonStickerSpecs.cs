using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class JsonStickerSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_PNG_sticker()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670623158943754")
        );

        // Assert
        var sticker = message.GetProperty("stickers").EnumerateArray().Single();

        sticker.GetProperty("id").GetString().Should().Be("904215665597120572");
        sticker.GetProperty("name").GetString().Should().Be("rock");
        sticker.GetProperty("format").GetString().Should().Be("Apng");
        sticker
            .GetProperty("sourceUrl")
            .GetString()
            .Should()
            .StartWith("https://cdn.discordapp.com/stickers/904215665597120572.png");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_Lottie_sticker()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.StickerTestCases,
            Snowflake.Parse("939670526517997590")
        );

        // Assert
        var sticker = message.GetProperty("stickers").EnumerateArray().Single();

        sticker.GetProperty("id").GetString().Should().Be("816087132447178774");
        sticker.GetProperty("name").GetString().Should().Be("Yikes");
        sticker.GetProperty("format").GetString().Should().Be("Lottie");
        sticker
            .GetProperty("sourceUrl")
            .GetString()
            .Should()
            .StartWith("https://cdn.discordapp.com/stickers/816087132447178774.json");
    }
}
