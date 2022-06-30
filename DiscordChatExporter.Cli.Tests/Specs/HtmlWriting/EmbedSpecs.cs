using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public class EmbedSpecs : IClassFixture<ExportWrapperFixture>
{
    private readonly ExportWrapperFixture _exportWrapper;

    public EmbedSpecs(ExportWrapperFixture exportWrapper)
    {
        _exportWrapper = exportWrapper;
    }

    [Fact]
    public async Task Message_with_an_embed_is_rendered_correctly()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866769910729146400")
        );

        // Assert
        message.Text().Should().ContainAll(
            "Embed author",
            "Embed title",
            "Embed description",
            "Field 1", "Value 1",
            "Field 2", "Value 2",
            "Field 3", "Value 3",
            "Embed footer"
        );
    }

    [Fact]
    public async Task Message_containing_an_image_link_is_rendered_with_an_image_embed()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991768701126852638")
        );

        // Assert
        message
            .QuerySelectorAll("img")
            .Select(e => e.GetAttribute("src"))
            .Should()
            .Contain("https://i.redd.it/f8w05ja8s4e61.png");
    }

    [Fact]
    public async Task Message_containing_an_image_link_and_nothing_else_is_rendered_without_text_content()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991768701126852638")
        );

        // Assert
        var content = message.QuerySelector(".chatlog__content")?.Text();
        content.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Message_containing_a_Spotify_track_link_is_rendered_with_a_track_embed()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("867886632203976775")
        );

        // Assert
        var iframeUrl = message.QuerySelector("iframe")?.GetAttribute("src");
        iframeUrl.Should().StartWith("https://open.spotify.com/embed/track/1LHZMWefF9502NPfArRfvP");
    }

    [Fact]
    public async Task Message_containing_a_YouTube_video_link_is_rendered_with_a_video_embed()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866472508588294165")
        );

        // Assert
        var iframeUrl = message.QuerySelector("iframe")?.GetAttribute("src");
        iframeUrl.Should().StartWith("https://www.youtube.com/embed/qOWW4OlgbvE");
    }
}