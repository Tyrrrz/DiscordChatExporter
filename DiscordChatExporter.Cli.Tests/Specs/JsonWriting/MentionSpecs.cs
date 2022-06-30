using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.JsonWriting;

public class MentionSpecs : IClassFixture<ExportWrapperFixture>
{
    private readonly ExportWrapperFixture _exportWrapper;

    public MentionSpecs(ExportWrapperFixture exportWrapper)
    {
        _exportWrapper = exportWrapper;
    }

    [Fact]
    public async Task User_mention_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsJsonAsync(
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
    public async Task Text_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsJsonAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459040480624680")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Text channel mention: #mention-tests");
    }

    [Fact]
    public async Task Voice_channel_mention_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsJsonAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459175462633503")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Voice channel mention: #chaos-vc [voice]");
    }

    [Fact]
    public async Task Role_mention_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsJsonAsync(
            ChannelIds.MentionTestCases,
            Snowflake.Parse("866459254693429258")
        );

        // Assert
        message.GetProperty("content").GetString().Should().Be("Role mention: @Role 1");
    }
}