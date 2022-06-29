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
    public async Task Message_with_a_link_to_an_image_is_rendered_with_that_image()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991758772349440053")
        );

        var imageSrc = message.QuerySelector("img")?.GetAttribute("src");

        // Assert
        imageSrc.Should().StartWithEquivalentOf("https://i.redd.it/f8w05ja8s4e61.png");
    }

    [Fact]
    public async Task Message_with_a_Spotify_track_is_rendered_using_an_iframe()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("867886632203976775")
        );

        var iframeSrc = message.QuerySelector("iframe")?.GetAttribute("src");

        // Assert
        iframeSrc.Should().StartWithEquivalentOf("https://open.spotify.com/embed/track/1LHZMWefF9502NPfArRfvP");
    }

    [Fact]
    public async Task Message_with_a_YouTube_video_is_rendered_using_an_iframe()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866472508588294165")
        );

        var iframeSrc = message.QuerySelector("iframe")?.GetAttribute("src");

        // Assert
        iframeSrc.Should().StartWithEquivalentOf("https://www.youtube.com/embed/qOWW4OlgbvE");
    }
}