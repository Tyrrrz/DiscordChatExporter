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
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/537

        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991768701126852638")
        );

        // Assert
        message
            .QuerySelectorAll("img")
            .Select(e => e.GetAttribute("src"))
            .Where(s => s?.EndsWith("i.redd.it/f8w05ja8s4e61.png") ?? false)
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task Message_containing_an_image_link_and_nothing_else_is_rendered_without_text_content()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/682

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
    public async Task Message_containing_a_gifv_link_is_rendered_with_a_video_embed()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("1019234520349814794")
        );

        // Assert
        message
            .QuerySelectorAll("source")
            .Select(e => e.GetAttribute("src"))
            .Where(s => s?.EndsWith("media.tenor.com/DDAJeW6BQKkAAAPo/tooncasm-test-copy.mp4") ?? false)
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task Message_containing_a_gifv_link_and_nothing_else_is_rendered_without_text_content()
    {
        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("1019234520349814794")
        );

        // Assert
        var content = message.QuerySelector(".chatlog__content")?.Text();
        content.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Message_containing_a_Spotify_track_link_is_rendered_with_a_track_embed()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/657

        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("867886632203976775")
        );

        // Assert
        var iframeUrl = message.QuerySelector("iframe")?.GetAttribute("src");
        iframeUrl.Should().Be("https://open.spotify.com/embed/track/1LHZMWefF9502NPfArRfvP");
    }

    [Fact]
    public async Task Message_containing_a_YouTube_video_link_is_rendered_with_a_video_embed()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/570

        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866472508588294165")
        );

        // Assert
        var iframeUrl = message.QuerySelector("iframe")?.GetAttribute("src");
        iframeUrl.Should().Be("https://www.youtube.com/embed/qOWW4OlgbvE");
    }

    [Fact]
    public async Task Message_containing_a_Twitter_post_link_with_multiple_images_is_rendered_as_a_single_embed()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/695

        // Act
        var message = await _exportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991757444017557665")
        );

        // Assert
        message
            .QuerySelectorAll("img")
            .Select(e => e.GetAttribute("src"))
            .Should()
            .ContainInOrder(
                "https://images-ext-1.discordapp.net/external/-n--xW3EHH_3jlrheVkMXHCM7T86b5Ty4-MzXCT4m1Q/https/pbs.twimg.com/media/FVYIzYPWAAAMBqZ.png",
                "https://images-ext-2.discordapp.net/external/z5nEmGeEldV-kswydGLhqUsFHbb5AWHtdvc9XT6N5rE/https/pbs.twimg.com/media/FVYJBWJWAAMNAx2.png",
                "https://images-ext-2.discordapp.net/external/gnip03SawMB6uZLagN5sRDpA_1Ap1CcEhMbJfK1z6WQ/https/pbs.twimg.com/media/FVYJHiRX0AANZcz.png",
                "https://images-ext-2.discordapp.net/external/jl1v6cCbLaGmiwmKU-ZkXnF4cFsJ39f9A3-oEdqPdZs/https/pbs.twimg.com/media/FVYJNZNXwAAPnVG.png"
            );

        message.QuerySelectorAll(".chatlog__embed").Should().ContainSingle();
    }
}