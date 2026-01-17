using System;
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
            ShouldDownloadAssets = true,
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

    [Fact]
    public async Task I_can_export_a_channel_and_download_all_referenced_assets_with_nested_paths()
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
            ShouldDownloadAssets = true,
            ShouldUseNestedMediaFilePaths = true,
        }.ExecuteAsync(new FakeConsole());

        // DEBUG: Print what's in the HTML vs what exists
        var srcPaths = Html.Parse(await File.ReadAllTextAsync(filePath))
            .QuerySelectorAll("body [src]")
            .Select(e => e.GetAttribute("src")!)
            .ToList();

        Console.WriteLine("=== SRC paths in HTML ===");
        foreach (var src in srcPaths)
        {
            var fullPath = Path.GetFullPath(src, dir.Path);
            var exists = File.Exists(fullPath);
            Console.WriteLine($"{(exists ? "✓" : "✗")} {src}");
            Console.WriteLine($"    Full: {fullPath}");
        }

        Console.WriteLine("\n=== Files actually on disk ===");
        foreach (var file in Directory.EnumerateFiles(dir.Path, "*", SearchOption.AllDirectories))
            Console.WriteLine(file);

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
