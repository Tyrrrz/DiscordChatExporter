using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using FluentAssertions;
using JsonExtensions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class FilterSpecs : IClassFixture<TempOutputFixture>
{
    private readonly TempOutputFixture _tempOutput;

    public FilterSpecs(TempOutputFixture tempOutput)
    {
        _tempOutput = tempOutput;
    }

    [Fact]
    public async Task Messages_filtered_by_text_only_include_messages_that_contain_that_text()
    {
        // Arrange
        var filePath = _tempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.FilterTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            MessageFilter = MessageFilter.Parse("some text")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json
            .Parse(await File.ReadAllTextAsync(filePath))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .ContainSingle("Some random text");
    }

    [Fact]
    public async Task Messages_filtered_by_author_only_include_messages_sent_by_that_author()
    {
        // Arrange
        var filePath = _tempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.FilterTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            MessageFilter = MessageFilter.Parse("from:Tyrrrz")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json
            .Parse(await File.ReadAllTextAsync(filePath))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("author").GetProperty("name").GetString())
            .Should()
            .AllBe("Tyrrrz");
    }

    [Fact]
    public async Task Messages_filtered_by_content_only_include_messages_that_have_that_content()
    {
        // Arrange
        var filePath = _tempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.FilterTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            MessageFilter = MessageFilter.Parse("has:image")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json
            .Parse(await File.ReadAllTextAsync(filePath))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .ContainSingle("This has image");
    }

    [Fact]
    public async Task Messages_filtered_by_pin_only_include_messages_that_have_been_pinned()
    {
        // Arrange
        var filePath = _tempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.FilterTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            MessageFilter = MessageFilter.Parse("has:pin")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json
            .Parse(await File.ReadAllTextAsync(filePath))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .ContainSingle("This is pinned");
    }

    [Fact]
    public async Task Messages_filtered_by_mention_only_include_messages_that_have_that_mention()
    {
        // Arrange
        var filePath = _tempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.FilterTestCases },
            ExportFormat = ExportFormat.Json,
            OutputPath = filePath,
            MessageFilter = MessageFilter.Parse("mentions:Tyrrrz")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Json
            .Parse(await File.ReadAllTextAsync(filePath))
            .GetProperty("messages")
            .EnumerateArray()
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .ContainSingle("This has mention");
    }
}