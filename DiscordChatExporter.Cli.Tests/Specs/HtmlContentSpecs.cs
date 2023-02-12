using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlContentSpecs
{
    [Fact]
    public async Task Messages_are_exported_correctly()
    {
        // Act
        var messages = await ExportWrapper.GetMessagesAsHtmlAsync(ChannelIds.DateRangeTestCases);

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

    [Fact]
    public async Task Messages_cross_posted_from_other_guilds_are_rendered_with_the_server_tag()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/633

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.ReplyTestCases,
            Snowflake.Parse("1072165330853576876")
        );

        // Assert
        message.Text().Should().Contain("This is a test message from an announcement channel on another server");
        message.Text().Should().Contain("SERVER");
        message.QuerySelector(".chatlog__reply-link").Should().BeNull();
    }
}