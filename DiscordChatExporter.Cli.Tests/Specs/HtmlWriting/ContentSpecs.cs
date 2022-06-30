using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public class ContentSpecs : IClassFixture<ExportWrapperFixture>
{
    private readonly ExportWrapperFixture _exportWrapper;

    public ContentSpecs(ExportWrapperFixture exportWrapper)
    {
        _exportWrapper = exportWrapper;
    }

    [Fact]
    public async Task Messages_are_exported_correctly()
    {
        // Act
        var messages = await _exportWrapper.GetMessagesAsHtmlAsync(ChannelIds.DateRangeTestCases);

        // Assert
        messages.Select(e => e.GetAttribute("data-message-id")).Should().Equal(
            "866674314627121232",
            "866710679758045195",
            "866732113319428096",
            "868490009366396958",
            "868505966528835604",
            "868505969821364245",
            "868505973294268457",
            "885169254029213696"
        );

        messages.SelectMany(e => e.Text()).Should().ContainInOrder(
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