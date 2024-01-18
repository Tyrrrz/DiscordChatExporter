using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using FluentAssertions;
using JsonExtensions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class FilterSpecs
{
    [Fact]
    public async Task I_can_filter_the_export_to_only_include_messages_that_contain_the_specified_text()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.FilterTestCases],
            ExportFormat = ExportFormat.Json,
            OutputPath = file.Path,
            MessageFilter = MessageFilter.Parse("some text")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json.Parse(await File.ReadAllTextAsync(file.Path))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .AllSatisfy(c => c.Contains("Some random text", StringComparison.Ordinal));
    }

    [Fact]
    public async Task I_can_filter_the_export_to_only_include_messages_that_were_sent_by_the_specified_author()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.FilterTestCases],
            ExportFormat = ExportFormat.Json,
            OutputPath = file.Path,
            MessageFilter = MessageFilter.Parse("from:Tyrrrz")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json.Parse(await File.ReadAllTextAsync(file.Path))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("author").GetProperty("name").GetString())
            .Should()
            .AllBe("tyrrrz");
    }

    [Fact]
    public async Task I_can_filter_the_export_to_only_include_messages_that_contain_images()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.FilterTestCases],
            ExportFormat = ExportFormat.Json,
            OutputPath = file.Path,
            MessageFilter = MessageFilter.Parse("has:image")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json.Parse(await File.ReadAllTextAsync(file.Path))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .AllSatisfy(c => c.Contains("This has image", StringComparison.Ordinal));
    }

    [Fact]
    public async Task I_can_filter_the_export_to_only_include_messages_that_have_been_pinned()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.FilterTestCases],
            ExportFormat = ExportFormat.Json,
            OutputPath = file.Path,
            MessageFilter = MessageFilter.Parse("has:pin")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json.Parse(await File.ReadAllTextAsync(file.Path))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .AllSatisfy(c => c.Contains("This is pinned", StringComparison.Ordinal));
    }

    [Fact]
    public async Task I_can_filter_the_export_to_only_include_messages_that_contain_guild_invites()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.FilterTestCases],
            ExportFormat = ExportFormat.Json,
            OutputPath = file.Path,
            MessageFilter = MessageFilter.Parse("has:invite")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json.Parse(await File.ReadAllTextAsync(file.Path))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .AllSatisfy(c => c.Contains("This has invite", StringComparison.Ordinal));
    }

    [Fact]
    public async Task I_can_filter_the_export_to_only_include_messages_that_contain_the_specified_mention()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.FilterTestCases],
            ExportFormat = ExportFormat.Json,
            OutputPath = file.Path,
            MessageFilter = MessageFilter.Parse("mentions:Tyrrrz")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json.Parse(await File.ReadAllTextAsync(file.Path))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .AllSatisfy(c => c.Contains("This has mention", StringComparison.Ordinal));
    }
}
