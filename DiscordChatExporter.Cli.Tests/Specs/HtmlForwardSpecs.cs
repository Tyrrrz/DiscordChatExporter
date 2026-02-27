using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils.Extensions;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlForwardSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_forwarded_message()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ForwardTestCases,
            Snowflake.Parse("1455202427115536514")
        );

        // Assert
        message
            .Text()
            .ReplaceWhiteSpace()
            .Should()
            .ContainAll("Forwarded", @"¯\_(ツ)_/¯", "12/29/2025 2:14 PM");
    }
}
