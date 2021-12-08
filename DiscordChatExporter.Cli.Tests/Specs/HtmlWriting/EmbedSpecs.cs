using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Fixtures;
using DiscordChatExporter.Cli.Tests.TestData;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs.HtmlWriting;

public record EmbedSpecs(ExportWrapperFixture ExportWrapper) : IClassFixture<ExportWrapperFixture>
{
    [Fact]
    public async Task Message_with_an_embed_is_rendered_correctly()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
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
    public async Task Message_with_a_Spotify_track_is_rendered_using_an_iframe()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
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
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866472508588294165")
        );

        var iframeSrc = message.QuerySelector("iframe")?.GetAttribute("src");

        // Assert
        iframeSrc.Should().StartWithEquivalentOf("https://www.youtube.com/embed/qOWW4OlgbvE");
    }
}