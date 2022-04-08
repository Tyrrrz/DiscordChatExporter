using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public record ReplySpecs(ExportWrapperFixture ExportWrapper) : IClassFixture<ExportWrapperFixture>
{
    [Fact]
    public async Task Reply_to_a_normal_message_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866460738239725598")
        );

        // Assert
        message.Text().Trim().Should().Contain("reply to original");
        message.QuerySelector(".chatlog__reference-link")?.Text().Trim().Should().Contain("original");
    }

    [Fact]
    public async Task Reply_to_a_deleted_message_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866460975388819486")
        );

        // Assert
        message.Text().Trim().Should().Contain("reply to deleted");
        message.QuerySelector(".chatlog__reference-link")?.Text().Trim().Should()
            .Contain("Original message was deleted or could not be loaded.");
    }

    [Fact]
    public async Task Reply_to_an_empty_message_with_attachment_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866462470335627294")
        );

        // Assert
        message.Text().Trim().Should().Contain("reply to attachment");
        message.QuerySelector(".chatlog__reference-link")?.Text().Trim().Should()
            .Contain("Click to see attachment");
    }
}