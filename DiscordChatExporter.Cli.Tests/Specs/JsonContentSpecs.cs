using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using JsonExtensions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class JsonContentSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_in_the_JSON_format()
    {
        // Act
        var messages = await ExportWrapper.GetMessagesAsJsonAsync(ChannelIds.DateRangeTestCases);

        // Assert
        messages
            .Select(j => j.GetProperty("id").GetString())
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
            .Select(j => j.GetProperty("content").GetString())
            .Should()
            .Equal(
                "Hello world",
                "Goodbye world",
                "Foo bar",
                "Hurdle Durdle",
                "One",
                "Two",
                "Three",
                "Yeet"
            );
    }

    [Fact]
    public async Task I_can_export_a_channel_in_the_JSON_format_with_messages_in_reverse_order()
    {
        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.DateRangeTestCases],
            ExportFormat = ExportFormat.Json,
            OutputPath = file.Path,
            Locale = "en-US",
            IsUtcNormalizationEnabled = true,
            MessageOrder = MessageOrder.Reverse,
        }.ExecuteAsync(new FakeConsole());

        var messages = Json.Parse(await File.ReadAllTextAsync(file.Path))
            .GetProperty("messages")
            .EnumerateArray()
            .ToArray();

        // Assert: messages should be in reverse chronological order
        messages
            .Select(j => j.GetProperty("id").GetString())
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
