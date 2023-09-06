using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlMarkdownSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323136411078787")
        );

        // Assert
        message.Text().Should().Contain("Default timestamp: 2/12/2023 1:36 PM");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker_in_the_short_format()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323205268967596")
        );

        // Assert
        message.Text().Should().Contain("Short time timestamp: 1:36 PM");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker_in_the_long_format()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323235342139483")
        );

        // Assert
        message.Text().Should().Contain("Long time timestamp: 1:36:12 PM");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker_in_the_short_date_format()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323326727634984")
        );

        // Assert
        message.Text().Should().Contain("Short date timestamp: 2/12/2023");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker_in_the_long_date_format()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323350731640863")
        );

        // Assert
        message.Text().Should().Contain("Long date timestamp: Sunday, February 12, 2023");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker_in_the_full_format()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323374379118593")
        );

        // Assert
        message.Text().Should().Contain("Full timestamp: Sunday, February 12, 2023 1:36 PM");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker_in_the_full_long_format()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323409095376947")
        );

        // Assert
        message
            .Text()
            .Should()
            .Contain("Full long timestamp: Sunday, February 12, 2023 1:36:12 PM");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_timestamp_marker_in_the_relative_format()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074323436853285004")
        );

        // Assert
        message.Text().Should().Contain("Relative timestamp: 2/12/2023 1:36 PM");
        message.InnerHtml.Should().Contain("Sunday, February 12, 2023 1:36 PM");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_an_invalid_timestamp_marker()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.MarkdownTestCases,
            Snowflake.Parse("1074328534409019563")
        );

        // Assert
        message.Text().Should().Contain("Invalid timestamp: Invalid date");
    }
}
