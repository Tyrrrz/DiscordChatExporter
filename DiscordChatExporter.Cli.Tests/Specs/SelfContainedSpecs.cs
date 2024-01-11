using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class SelfContainedSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_and_download_all_referenced_assets()
    {
        // Arrange
        using var dir = TempDir.Create();
        var filePath = Path.Combine(dir.Path, "output.html");

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.SelfContainedTestCases],
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = filePath,
            ShouldDownloadAssets = true
        }.ExecuteAsync(new FakeConsole());

        // Assert
        Html.Parse(await File.ReadAllTextAsync(filePath))
            .QuerySelectorAll("body [src]")
            .Select(e => e.GetAttribute("src")!)
            .Select(f => Path.GetFullPath(f, dir.Path))
            .All(File.Exists)
            .Should()
            .BeTrue();
    }
}
