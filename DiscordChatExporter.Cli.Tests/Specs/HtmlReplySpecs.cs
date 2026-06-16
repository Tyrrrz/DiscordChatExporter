using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlReplySpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_that_replies_to_another_message()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866460738239725598")
        );

        // Assert
        message.Text().Should().Contain("reply to original");
        message.QuerySelector(".chatlog__reply-link")?.Text().Should().Contain("original");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_that_replies_to_a_deleted_message()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/645

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866460975388819486")
        );

        // Assert
        message.Text().Should().Contain("reply to deleted");
        message
            .QuerySelector(".chatlog__reply-link")
            ?.Text()
            .Should()
            .Contain("Original message was deleted or could not be loaded.");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_that_replies_to_an_empty_message_with_an_attachment()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/634

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866462470335627294")
        );

        // Assert
        message.Text().Should().Contain("reply to attachment");
        message
            .QuerySelector(".chatlog__reply-link")
            ?.Text()
            .Should()
            .Contain("Click to see attachment");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_that_replies_to_an_interaction()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/569

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("1075152916417085492")
        );

        // Assert
        message.Text().Should().Contain("used /poll");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_cross_posted_from_another_guild()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/633

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("1072165330853576876")
        );

        // Assert
        message
            .Text()
            .Should()
            .Contain("This is a test message from an announcement channel on another server");
        message.Text().Should().Contain("SERVER");
        message.QuerySelector(".chatlog__reply-link").Should().BeNull();
    }
}
