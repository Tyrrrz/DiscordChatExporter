using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.CsvWriting;

public record ContentSpecs(ExportWrapperFixture ExportWrapper) : IClassFixture<ExportWrapperFixture>
{
    [Fact]
    public async Task Messages_are_exported_correctly()
    {
        // Act
        var document = await ExportWrapper.ExportAsCsvAsync(ChannelIds.DateRangeTestCases);

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