using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlContentSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_in_the_HTML_format()
    {
        // Act
        var messages = await ExportWrapper.GetMessagesAsHtmlAsync(ChannelIds.DateRangeTestCases);

        // Assert
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
    }
}
