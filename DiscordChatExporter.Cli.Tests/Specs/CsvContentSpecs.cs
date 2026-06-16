using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class CsvContentSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_in_the_CSV_format()
    {
        // Act
        var document = await ExportWrapper.ExportAsCsvAsync(ChannelIds.DateRangeTestCases);

        // Assert
        document
            .Should()
            .ContainAll(
                "tyrrrz",
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
