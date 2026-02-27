using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class PlainTextForwardSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_forwarded_message()
    {
        // Act
        var document = await ExportWrapper.ExportAsPlainTextAsync(ChannelIds.ForwardTestCases);

        // Assert
        document
            .Should()
            .ContainAll("{Forwarded Message}", @"¯\_(ツ)_/¯", "December 29, 2025");
    }
}
