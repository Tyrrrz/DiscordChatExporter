using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class PlainTextContentSpecs
{
    [Fact]
    public async Task Messages_are_exported_correctly()
    {
        // Act
        var document = await ExportWrapper.ExportAsPlainTextAsync(ChannelIds.DateRangeTestCases);

        // Assert
        document.Should().ContainAll(
            "Tyrrrz#5447",
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
}