using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using PowerKit;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlContentSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_in_the_HTML_format()
    {
        // Act
        var document = await ExportWrapper.ExportAsHtmlAsync(ChannelIds.DateRangeTestCases);
        var chatlog = document.QuerySelector(".chatlog");
        var messages = document.QuerySelectorAll("[data-message-id]").ToArray();

        // Assert
        chatlog.Should().NotBeNull();
        chatlog!.GetAttribute("data-machine-metadata-version").Should().Be("1");
        chatlog
            .GetAttribute("data-channel-id")
            .Should()
            .Be(ChannelIds.DateRangeTestCases.ToString());

        messages
            .Select(e => e.GetAttribute("data-message-id"))
            .Should()
            .Equal(
                "866674314627121232",
                "866710679758045195",
                "866732113319428096",
                "868490009366396958",
                "868505966528835604",
                "868505969821364245",
                "868505973294268457",
                "885169254029213696"
            );

        messages
            .SelectMany(e => e.Text())
            .Should()
            .ContainInOrder(
                "Hello world",
                "Goodbye world",
                "Foo bar",
                "Hurdle Durdle",
                "One",
                "Two",
                "Three",
                "Yeet"
            );

        foreach (var message in messages)
        {
            message.GetAttribute("data-message-type").Should().NotBeNullOrWhiteSpace();
            message.GetAttribute("data-author-id").Should().NotBeNullOrWhiteSpace();
            message.GetAttribute("data-author-name").Should().NotBeNullOrWhiteSpace();
            message.GetAttribute("data-author-display-name").Should().NotBeNullOrWhiteSpace();

            DateTimeOffset
                .TryParseExact(
                    message.GetAttribute("data-timestamp"),
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out _
                )
                .Should()
                .BeTrue();
        }
    }

    [Fact]
    public async Task I_can_disable_machine_metadata_in_the_HTML_format()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.DateRangeTestCases],
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = file.Path,
            Locale = "en-US",
            IsUtcNormalizationEnabled = true,
            ShouldIncludeMachineMetadata = false,
        }.ExecuteAsync(new FakeConsole());

        var document = Html.Parse(await File.ReadAllTextAsync(file.Path));
        var chatlog = document.QuerySelector(".chatlog");
        var messages = document.QuerySelectorAll("[data-message-id]");

        // Assert
        chatlog.Should().NotBeNull();
        chatlog!.HasAttribute("data-machine-metadata-version").Should().BeFalse();

        foreach (var message in messages)
        {
            message.HasAttribute("data-author-id").Should().BeFalse();
            message.HasAttribute("data-timestamp").Should().BeFalse();
        }
    }

    [Fact]
    public async Task I_can_export_a_channel_in_the_HTML_format_in_the_reverse_order()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.DateRangeTestCases],
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = file.Path,
            Locale = "en-US",
            IsUtcNormalizationEnabled = true,
            IsReverseMessageOrder = true,
        }.ExecuteAsync(new FakeConsole());

        var document = Html.Parse(await File.ReadAllTextAsync(file.Path));
        var messages = document.QuerySelectorAll("[data-message-id]").ToArray();

        // Assert
        messages
            .Select(e => e.GetAttribute("data-message-id"))
            .Should()
            .Equal(
                "885169254029213696",
                "868505973294268457",
                "868505969821364245",
                "868505966528835604",
                "868490009366396958",
                "866732113319428096",
                "866710679758045195",
                "866674314627121232"
            );
    }
}
