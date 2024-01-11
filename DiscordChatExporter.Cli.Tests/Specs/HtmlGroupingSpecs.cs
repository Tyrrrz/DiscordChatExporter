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
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlGroupingSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_and_the_messages_are_grouped_according_to_their_author_and_timestamps()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/152

        // Arrange
        using var file = TempFile.Create();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = [ChannelIds.GroupingTestCases],
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = file.Path
        }.ExecuteAsync(new FakeConsole());

        // Assert
        var messageGroups = Html.Parse(await File.ReadAllTextAsync(file.Path))
            .QuerySelectorAll(".chatlog__message-group");

        messageGroups.Should().HaveCount(2);

        messageGroups[0]
            .QuerySelectorAll(".chatlog__content")
            .Select(e => e.Text())
            .Should()
            .ContainInOrder(
                "First",
                "Second",
                "Third",
                "Fourth",
                "Fifth",
                "Sixth",
                "Seventh",
                "Eighth",
                "Ninth",
                "Tenth"
            );

        messageGroups[1]
            .QuerySelectorAll(".chatlog__content")
            .Select(e => e.Text())
            .Should()
            .ContainInOrder("Eleventh", "Twelveth", "Thirteenth", "Fourteenth", "Fifteenth");
    }
}
