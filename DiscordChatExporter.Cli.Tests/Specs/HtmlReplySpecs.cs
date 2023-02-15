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
    public async Task Message_with_a_reply_is_rendered_correctly()
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
    public async Task Message_with_a_reply_to_a_deleted_message_is_rendered_correctly()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/645

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866460975388819486")
        );

        // Assert
        message.Text().Should().Contain("reply to deleted");
        message.QuerySelector(".chatlog__reply-link")?.Text().Should().Contain(
            "Original message was deleted or could not be loaded."
        );
    }

    [Fact]
    public async Task Message_with_a_reply_to_an_empty_message_with_attachment_is_rendered_correctly()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/634

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866462470335627294")
        );

        // Assert
        message.Text().Should().Contain("reply to attachment");
        message.QuerySelector(".chatlog__reply-link")?.Text().Should().Contain("Click to see attachment");
    }

    [Fact]
    public async Task Message_with_a_reply_to_an_interaction_is_rendered_correctly()
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
}