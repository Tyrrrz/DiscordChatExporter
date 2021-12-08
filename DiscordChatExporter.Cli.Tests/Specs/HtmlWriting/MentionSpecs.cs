using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public record MentionSpecs(ExportWrapperFixture ExportWrapper) : IClassFixture<ExportWrapperFixture>
{
    [Fact]
    public async Task User_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866458840245076028")
        );

        // Assert
        message.Text().Trim().Should().Be("User mention: @Tyrrrz");
        message.InnerHtml.Should().Contain("Tyrrrz#5447");
    }

    [Fact]
    public async Task Text_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459040480624680")
        );

        // Assert
        message.Text().Trim().Should().Be("Text channel mention: #mention-tests");
    }

    [Fact]
    public async Task Voice_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459175462633503")
        );

        // Assert
        message.Text().Trim().Should().Be("Voice channel mention: 🔊chaos-vc");
    }

    [Fact]
    public async Task Role_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459254693429258")
        );

        // Assert
        message.Text().Trim().Should().Be("Role mention: @Role 1");
    }
}