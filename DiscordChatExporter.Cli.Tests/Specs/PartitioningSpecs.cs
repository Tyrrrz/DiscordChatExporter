using System.IO;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Partitioning;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class PartitioningSpecs
{
    [Fact]
    public async Task Messages_partitioned_by_count_are_split_into_a_corresponding_number_of_files()
    {
        // Arrange
        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "output.html");

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.DateRangeTestCases },
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = filePath,
            PartitionLimit = PartitionLimit.Parse("3")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Directory.EnumerateFiles(dir.Path, "output*")
            .Should()
            .HaveCount(3);
    }

    [Fact]
    public async Task Messages_partitioned_by_file_size_are_split_into_a_corresponding_number_of_files()
    {
        // Arrange
        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "output.html");

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.DateRangeTestCases },
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = filePath,
            PartitionLimit = PartitionLimit.Parse("1kb")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Directory.EnumerateFiles(dir.Path, "output*")
            .Should()
            .HaveCount(8);
    }
}