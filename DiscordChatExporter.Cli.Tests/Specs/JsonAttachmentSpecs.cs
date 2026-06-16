using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class JsonAttachmentSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_generic_attachment()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885587844989612074")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Generic file attachment");

        var attachments = message.GetProperty("attachments").EnumerateArray().ToArray();
        attachments.Should().HaveCount(1);

        attachments[0]
            .GetProperty("url")
            .GetString()
            .Should()
            .StartWith(
                "https://cdn.discordapp.com/attachments/885587741654536192/885587844964417596/Test.txt"
            );

        attachments[0].GetProperty("fileName").GetString().Should().Be("Test.txt");
        attachments[0].GetProperty("fileSizeBytes").GetInt64().Should().Be(11);
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_an_image_attachment()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885654862656843786")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Image attachment");

        var attachments = message.GetProperty("attachments").EnumerateArray().ToArray();
        attachments.Should().HaveCount(1);

        attachments[0]
            .GetProperty("url")
            .GetString()
            .Should()
            .StartWith(
                "https://cdn.discordapp.com/attachments/885587741654536192/885654862430359613/bird-thumbnail.png"
            );

        attachments[0].GetProperty("fileName").GetString().Should().Be("bird-thumbnail.png");
        attachments[0].GetProperty("fileSizeBytes").GetInt64().Should().Be(466335);
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_video_attachment()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885655761919836171")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Video attachment");

        var attachments = message.GetProperty("attachments").EnumerateArray().ToArray();
        attachments.Should().HaveCount(1);

        attachments[0]
            .GetProperty("url")
            .GetString()
            .Should()
            .StartWith(
                "https://cdn.discordapp.com/attachments/885587741654536192/885655761512968233/file_example_MP4_640_3MG.mp4"
            );

        attachments[0]
            .GetProperty("fileName")
            .GetString()
            .Should()
            .Be("file_example_MP4_640_3MG.mp4");

        attachments[0].GetProperty("fileSizeBytes").GetInt64().Should().Be(3114374);
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_an_audio_attachment()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885656175620808734")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Audio attachment");

        var attachments = message.GetProperty("attachments").EnumerateArray().ToArray();
        attachments.Should().HaveCount(1);

        attachments[0]
            .GetProperty("url")
            .GetString()
            .Should()
            .StartWith(
                "https://cdn.discordapp.com/attachments/885587741654536192/885656175348187146/file_example_MP3_1MG.mp3"
            );

        attachments[0].GetProperty("fileName").GetString().Should().Be("file_example_MP3_1MG.mp3");
        attachments[0].GetProperty("fileSizeBytes").GetInt64().Should().Be(1087849);
    }
}
