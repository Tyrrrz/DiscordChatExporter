using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlMentionSpecs
{
    [Fact]
    public async Task Message_with_a_user_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866458840245076028")
        );

        // Assert
        message.Text().Should().Contain("User mention: @Tyrrrz");
        message.InnerHtml.Should().Contain("Tyrrrz#5447");
    }

    [Fact]
    public async Task Message_with_a_text_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459040480624680")
        );

        // Assert
        message.Text().Should().Contain("Text channel mention: #mention-tests");
    }

    [Fact]
    public async Task Message_with_a_voice_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459175462633503")
        );

        // Assert
        message.Text().Should().Contain("Voice channel mention: 🔊general");
    }

    [Fact]
    public async Task Message_with_a_role_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459254693429258")
        );

        // Assert
        message.Text().Should().Contain("Role mention: @Role 1");
    }
}