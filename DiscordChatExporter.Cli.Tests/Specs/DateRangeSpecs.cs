﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using JsonExtensions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public record DateRangeSpecs(TempOutputFixture TempOutput) : IClassFixture<TempOutputFixture>
{
    [Fact]
    public async Task Messages_filtered_after_specific_date_only_include_messages_sent_after_that_date()
    {
        // Arrange
        var after = new DateTimeOffset(2021, 07, 24, 0, 0, 0, TimeSpan.Zero);
        var filePath = TempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.DateRangeTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            After = Snowflake.FromDate(after)
        }.ExecuteAsync(new FakeConsole());

        var data = await File.ReadAllTextAsync(filePath);
        var document = Json.Parse(data);

        var timestamps = document
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("timestamp").GetDateTimeOffset())
            .ToArray();

        // Assert
        timestamps.All(t => t > after).Should().BeTrue();

        timestamps.Should().BeEquivalentTo(new[]
        {
            new DateTimeOffset(2021, 07, 24, 13, 49, 13, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 24, 14, 52, 38, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 24, 14, 52, 39, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 24, 14, 52, 40, TimeSpan.Zero),
            new DateTimeOffset(2021, 09, 08, 14, 26, 35, TimeSpan.Zero)
        }, o =>
        {
            return o
                .Using<DateTimeOffset>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))
                )
                .WhenTypeIs<DateTimeOffset>();
        });
    }

    [Fact]
    public async Task Messages_filtered_before_specific_date_only_include_messages_sent_before_that_date()
    {
        // Arrange
        var before = new DateTimeOffset(2021, 07, 24, 0, 0, 0, TimeSpan.Zero);
        var filePath = TempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.DateRangeTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            Before = Snowflake.FromDate(before)
        }.ExecuteAsync(new FakeConsole());

        var data = await File.ReadAllTextAsync(filePath);
        var document = Json.Parse(data);

        var timestamps = document
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("timestamp").GetDateTimeOffset())
            .ToArray();

        // Assert
        timestamps.All(t => t < before).Should().BeTrue();

        timestamps.Should().BeEquivalentTo(new[]
        {
            new DateTimeOffset(2021, 07, 19, 13, 34, 18, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 19, 15, 58, 48, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 19, 17, 23, 58, TimeSpan.Zero)
        }, o =>
        {
            return o
                .Using<DateTimeOffset>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))
                )
                .WhenTypeIs<DateTimeOffset>();
        });
    }

    [Fact]
    public async Task Messages_filtered_between_specific_dates_only_include_messages_sent_between_those_dates()
    {
        // Arrange
        var after = new DateTimeOffset(2021, 07, 24, 0, 0, 0, TimeSpan.Zero);
        var before = new DateTimeOffset(2021, 08, 01, 0, 0, 0, TimeSpan.Zero);
        var filePath = TempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.DateRangeTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            Before = Snowflake.FromDate(before),
            After = Snowflake.FromDate(after)
        }.ExecuteAsync(new FakeConsole());

        var data = await File.ReadAllTextAsync(filePath);
        var document = Json.Parse(data);

        var timestamps = document
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("timestamp").GetDateTimeOffset())
            .ToArray();

        // Assert
        timestamps.All(t => t < before && t > after).Should().BeTrue();

        timestamps.Should().BeEquivalentTo(new[]
        {
            new DateTimeOffset(2021, 07, 24, 13, 49, 13, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 24, 14, 52, 38, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 24, 14, 52, 39, TimeSpan.Zero),
            new DateTimeOffset(2021, 07, 24, 14, 52, 40, TimeSpan.Zero)
        }, o =>
        {
            return o
                .Using<DateTimeOffset>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))
                )
                .WhenTypeIs<DateTimeOffset>();
        });
    }
}