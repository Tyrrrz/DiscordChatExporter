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
            .ContainSingle("Some random text");
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
    public async Task I_can_filter_the_export_to_only_include_messages_that_contain_the_specified_content()
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
            .ContainSingle("This has image");
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
            .ContainSingle("This is pinned");
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
            .ContainSingle("This has mention");
    }
}
