using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlAttachmentSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_generic_attachment()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885587844989612074")
        );

        // Assert
        message.Text().Should().ContainAll("Generic file attachment", "Test.txt", "11 bytes");

        message
            .QuerySelectorAll("a")
            .Select(e => e.GetAttribute("href"))
            .Should()
            .Contain(u => u.Contains("Test.txt", StringComparison.Ordinal));
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_an_image_attachment()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885654862656843786")
        );

        // Assert
        message.Text().Should().Contain("Image attachment");

        message
            .QuerySelectorAll("img")
            .Select(e => e.GetAttribute("src"))
            .Should()
            .Contain(u => u.Contains("bird-thumbnail.png", StringComparison.Ordinal));
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_video_attachment()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/333

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885655761919836171")
        );

        // Assert
        message.Text().Should().Contain("Video attachment");

        var videoUrl = message.QuerySelector("video source")?.GetAttribute("src");
        videoUrl
            .Should()
            .StartWith(
                "https://cdn.discordapp.com/attachments/885587741654536192/885655761512968233/file_example_MP4_640_3MG.mp4"
            );
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_an_audio_attachment()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/333

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.AttachmentTestCases,
            Snowflake.Parse("885656175620808734")
        );

        // Assert
        message.Text().Should().Contain("Audio attachment");

        var audioUrl = message.QuerySelector("audio source")?.GetAttribute("src");
        audioUrl
            .Should()
            .StartWith(
                "https://cdn.discordapp.com/attachments/885587741654536192/885656175348187146/file_example_MP3_1MG.mp3"
            );
    }
}
