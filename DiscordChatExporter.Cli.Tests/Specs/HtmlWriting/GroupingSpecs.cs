using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Exporting;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public class GroupingSpecs : IClassFixture<TempOutputFixture>
{
    private readonly TempOutputFixture _tempOutput;

    public GroupingSpecs(TempOutputFixture tempOutput)
    {
        _tempOutput = tempOutput;
    }

    [Fact]
    public async Task Messages_are_grouped_correctly()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/152

        // Arrange
        var filePath = _tempOutput.GetTempFilePath();

        // Act
        await new ExportChannelsCommand
        {
            Token = Secrets.DiscordToken,
            ChannelIds = new[] { ChannelIds.GroupingTestCases },
            ExportFormat = ExportFormat.HtmlDark,
            OutputPath = filePath
        }.ExecuteAsync(new FakeConsole());

        // Assert
        var messageGroups = Html
            .Parse(await File.ReadAllTextAsync(filePath))
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
            .ContainInOrder(
                "Eleventh",
                "Twelveth",
                "Thirteenth",
                "Fourteenth",
                "Fifteenth"
            );
    }
}