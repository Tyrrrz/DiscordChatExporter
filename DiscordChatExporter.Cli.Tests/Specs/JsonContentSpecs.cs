using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using FluentAssertions;
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
}
