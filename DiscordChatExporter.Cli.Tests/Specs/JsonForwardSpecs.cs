using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class JsonForwardSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_forwarded_message()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.ForwardTestCases,
            Snowflake.Parse("1455202427115536514")
        );

        // Assert
        var reference = message.GetProperty("reference");
        reference.GetProperty("type").GetString().Should().Be("Forward");
        reference.GetProperty("guildId").GetString().Should().Be("869237470565392384");

        var forwardedMessage = message.GetProperty("forwardedMessage");
        forwardedMessage.GetProperty("content").GetString().Should().Contain(@"¯\_(ツ)_/¯");
        forwardedMessage.GetProperty("timestamp").GetString().Should().StartWith("2025-12-29");
    }
}

