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

public class PartitioningSpecs : IClassFixture<TempOutputFixture>
{
    private readonly TempOutputFixture _tempOutput;

    public PartitioningSpecs(TempOutputFixture tempOutput)
    {
        _tempOutput = tempOutput;
    }

    [Fact]
    public async Task Messages_partitioned_by_count_are_split_into_a_corresponding_number_of_files()
    {
        // Arrange
        var filePath = _tempOutput.GetTempFilePath();
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
    public async Task Messages_partitioned_by_file_size_are_split_into_a_corresponding_number_of_files()
    {
        // Arrange
        var filePath = _tempOutput.GetTempFilePath();
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var dirPath = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();

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
        Directory.EnumerateFiles(dirPath, fileNameWithoutExt + "*")
            .Should()
            .HaveCount(8);
    }
}