using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public record SelfContainedSpecs(TempOutputFixture TempOutput) : IClassFixture<TempOutputFixture>
{
    [Fact]
    public async Task Messages_in_self_contained_export_only_reference_local_file_resources()
    {
        // Arrange
        var filePath = TempOutput.GetTempFilePath();
        var dirPath = Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.SelfContainedTestCases },
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = filePath,
            ShouldDownloadMedia = true
        }.ExecuteAsync(new FakeConsole());

        var data = await File.ReadAllTextAsync(filePath);
        var document = Html.Parse(data);

        // Assert
        document
            .QuerySelectorAll("body [src]")
            .Select(e => e.GetAttribute("src")!)
            .Select(f => Path.GetFullPath(f, dirPath))
            .All(File.Exists)
            .Should()
            .BeTrue();
    }
}