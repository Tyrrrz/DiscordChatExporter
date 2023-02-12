using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class JsonMentionSpecs
{
    [Fact]
    public async Task Message_with_a_user_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866458840245076028")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("User mention: @Tyrrrz");

        message
            .GetProperty("mentions")
            .EnumerateArray()
            .Select(j => j.GetProperty("id").GetString())
            .Should()
            .Contain("128178626683338752");
    }

    [Fact]
    public async Task Message_with_a_text_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459040480624680")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Text channel mention: #mention-tests");
    }

    [Fact]
    public async Task Message_with_a_voice_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459175462633503")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Voice channel mention: #general [voice]");
    }

    [Fact]
    public async Task Message_with_a_role_mention_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459254693429258")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Role mention: @Role 1");
    }
}