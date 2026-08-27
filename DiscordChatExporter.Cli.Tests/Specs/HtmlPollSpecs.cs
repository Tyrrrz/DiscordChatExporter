using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using PowerKit.Extensions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlPollSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_poll()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.PollTestCases,
            Snowflake.Parse("1298034559048487016")
        );

        // Assert
        message
            .Text()
            .Should()
            .ContainAll(
                "Poll question",
                "No emoji",
                "1 vote",
                "20%",
                "Default emoji",
                "3 votes",
                "60%",
                "Custom emoji",
                "1 vote",
                "5 votes"
            );

        message
            .QuerySelectorAll("img")
            .Select(e => e.GetAttribute("title"))
            .Should()
            .Contain("heart")
            .And.Contain("dce");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_poll_result()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.PollTestCases,
            Snowflake.Parse("1298396967742996641")
        );

        // Assert
        message
            .Text()
            .Should()
            .ContainAll("Poll question", "Default emoji", "Winning answer", "60%");

        message.Text().ReplaceWhiteSpace(' ').Should().Contain("10/22/2024 9:26 PM");
    }
}
