using System;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlMarkdownSpecs
{
    [Fact]
    public async Task Message_with_a_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323136411078787")
            );

            // Assert
            message.Text().Should().Contain("Default timestamp: 02/12/2023 3:36 PM");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_a_short_time_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323205268967596")
            );

            // Assert
            message.Text().Should().Contain("Short time timestamp: 3:36 PM");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_a_long_time_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323235342139483")
            );

            // Assert
            message.Text().Should().Contain("Long time timestamp: 3:36:12 PM");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_a_short_date_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323326727634984")
            );

            // Assert
            message.Text().Should().Contain("Short date timestamp: 02/12/2023");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_a_long_date_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323350731640863")
            );

            // Assert
            message.Text().Should().Contain("Long date timestamp: February 12, 2023");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_a_full_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323374379118593")
            );

            // Assert
            message.Text().Should().Contain("Full timestamp: February 12, 2023 3:36 PM");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_a_full_long_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323409095376947")
            );

            // Assert
            message.Text().Should().Contain("Full long timestamp: Sunday, February 12, 2023 3:36 PM");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_a_relative_timestamp_is_rendered_as_the_default_timestamp()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074323436853285004")
            );

            // Assert
            message.Text().Should().Contain("Relative timestamp: 02/12/2023 3:36 PM");
            message.InnerHtml.Should().Contain("Sunday, February 12, 2023 3:36 PM");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }

    [Fact]
    public async Task Message_with_an_invalid_timestamp_is_rendered_correctly()
    {
        // Date formatting code relies on the local time zone, so we need to set it to a fixed value
        TimeZoneInfoEx.SetLocal(TimeSpan.FromHours(+2));

        try
        {
            // Act
            var message = await ExportWrapper.GetMessageAsHtmlAsync(
                ChannelIds.MarkdownTestCases,
                Snowflake.Parse("1074328534409019563")
            );

            // Assert
            message.Text().Should().Contain("Invalid timestamp: Invalid date");
        }
        finally
        {
            TimeZoneInfo.ClearCachedData();
        }
    }
}