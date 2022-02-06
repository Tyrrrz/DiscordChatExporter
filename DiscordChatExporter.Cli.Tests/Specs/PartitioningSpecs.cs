using System.IO;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Partitioning;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public record PartitioningSpecs(TempOutputFixture TempOutput) : IClassFixture<TempOutputFixture>
{
    [Fact]
    public async Task Messages_partitioned_by_count_are_split_into_multiple_files_correctly()
    {
        // Arrange
        var filePath = TempOutput.GetTempFilePath();
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var dirPath = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();

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
        Directory.EnumerateFiles(dirPath, fileNameWithoutExt + "*")
            .Should()
            .HaveCount(3);
    }

    [Fact]
    public async Task Messages_partitioned_by_file_size_are_split_into_multiple_files_correctly()
    {
        // Arrange
        var filePath = TempOutput.GetTempFilePath();
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var dirPath = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.DateRangeTestCases },
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = filePath,
            PartitionLimit = PartitionLimit.Parse("20kb")
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Directory.EnumerateFiles(dirPath, fileNameWithoutExt + "*")
            .Should()
            .HaveCount(3);
    }
}