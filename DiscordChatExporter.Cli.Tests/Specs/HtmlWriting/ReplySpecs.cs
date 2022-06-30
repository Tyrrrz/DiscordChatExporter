using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public class ReplySpecs : IClassFixture<ExportWrapperFixture>
{
    private readonly ExportWrapperFixture _exportWrapper;

    public ReplySpecs(ExportWrapperFixture exportWrapper)
    {
        _exportWrapper = exportWrapper;
    }

    [Fact]
    public async Task Reply_to_a_normal_message_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866460738239725598")
        );

        // Assert
        message.Text().Should().Contain("reply to original");
        message.QuerySelector(".chatlog__reference-link")?.Text().Should().Contain("original");
    }

    [Fact]
    public async Task Reply_to_a_deleted_message_is_rendered_correctly()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/645

        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866460975388819486")
        );

        // Assert
        message.Text().Should().Contain("reply to deleted");
        message.QuerySelector(".chatlog__reference-link")?.Text().Should().Contain(
            "Original message was deleted or could not be loaded."
        );
    }

    [Fact]
    public async Task Reply_to_an_empty_message_with_attachment_is_rendered_correctly()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/634

        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("866462470335627294")
        );

        // Assert
        message.Text().Should().Contain("reply to attachment");
        message.QuerySelector(".chatlog__reference-link")?.Text().Should().Contain("Click to see attachment");
    }
}