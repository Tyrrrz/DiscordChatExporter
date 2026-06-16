using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using FluentAssertions;
using PowerKit.Extensions;
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
            .ReplaceWhiteSpace(' ')
            .Should()
            .ContainAll("{Forwarded Message}", @"¯\_(ツ)_/¯", "12/28/2025 10:52 PM");
    }
}
