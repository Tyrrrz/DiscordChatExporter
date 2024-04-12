using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils.Extensions;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlEmbedSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_rich_embed()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866769910729146400")
        );

        // Assert
        message
            .Text()
            .Should()
            .ContainAll(
                "Embed author",
                "Embed title",
                "Embed description",
                "Field 1",
                "Value 1",
                "Field 2",
                "Value 2",
                "Field 3",
                "Value 3",
                "Embed footer"
            );
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_an_image_embed()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/537

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991768701126852638")
        );

        // Assert
        message
            .QuerySelectorAll("img")
            .Select(e => e.GetAttribute("src"))
            .WhereNotNull()
            .Where(s => s.Contains("f8w05ja8s4e61.png", StringComparison.Ordinal))
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_an_image_embed_and_the_text_is_hidden_if_it_only_contains_the_image_link()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/682

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991768701126852638")
        );

        // Assert
        var content = message.QuerySelector(".chatlog__content")?.Text();
        content.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_video_embed()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("1083751036596002856")
        );

        // Assert
        message
            .QuerySelectorAll("source")
            .Select(e => e.GetAttribute("src"))
            .WhereNotNull()
            .Where(s =>
                s.Contains(
                    "i_am_currently_feeling_slight_displeasure_of_what_you_have_just_sent_lqrem.mp4",
                    StringComparison.Ordinal
                )
            )
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_GIFV_embed()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("1019234520349814794")
        );

        // Assert
        message
            .QuerySelectorAll("source")
            .Select(e => e.GetAttribute("src"))
            .WhereNotNull()
            .Where(s => s.Contains("tooncasm-test-copy.mp4", StringComparison.Ordinal))
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_GIFV_embed_and_the_text_is_hidden_if_it_only_contains_the_video_link()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("1019234520349814794")
        );

        // Assert
        var content = message.QuerySelector(".chatlog__content")?.Text();
        content.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_Spotify_track_embed()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/657

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("867886632203976775")
        );

        // Assert
        var iframeUrl = message.QuerySelector("iframe")?.GetAttribute("src");
        iframeUrl.Should().StartWith("https://open.spotify.com/embed/track/1LHZMWefF9502NPfArRfvP");
    }

    [Fact(Skip = "Twitch does not allow embeds from inside local HTML files")]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_Twitch_clip_embed()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/1196

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("1207002986128216074")
        );

        // Assert
        var iframeUrl = message.QuerySelector("iframe")?.GetAttribute("src");
        iframeUrl
            .Should()
            .StartWith(
                "https://clips.twitch.tv/embed?clip=SpicyMildCiderThisIsSparta--PQhbllrvej_Ee7v"
            );
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_YouTube_video_embed()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/570

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866472508588294165")
        );

        // Assert
        var iframeUrl = message.QuerySelector("iframe")?.GetAttribute("src");
        iframeUrl.Should().StartWith("https://www.youtube.com/embed/qOWW4OlgbvE");
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_Twitter_post_embed_that_includes_multiple_images()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/695

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("991757444017557665")
        );

        // Assert
        var imageUrls = message
            .QuerySelectorAll("img")
            .Select(e => e.GetAttribute("src"))
            .ToArray();

        imageUrls
            .Should()
            .Contain(u =>
                u.EndsWith(
                    "https/pbs.twimg.com/media/FVYIzYPWAAAMBqZ.png",
                    StringComparison.Ordinal
                )
            );

        imageUrls
            .Should()
            .Contain(u =>
                u.EndsWith(
                    "https/pbs.twimg.com/media/FVYJBWJWAAMNAx2.png",
                    StringComparison.Ordinal
                )
            );

        imageUrls
            .Should()
            .Contain(u =>
                u.EndsWith(
                    "https/pbs.twimg.com/media/FVYJHiRX0AANZcz.png",
                    StringComparison.Ordinal
                )
            );

        imageUrls
            .Should()
            .Contain(u =>
                u.EndsWith(
                    "https/pbs.twimg.com/media/FVYJNZNXwAAPnVG.png",
                    StringComparison.Ordinal
                )
            );

        message.QuerySelectorAll(".chatlog__embed").Should().ContainSingle();
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_guild_invite()
    {
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/649

        // Act
        var message = await ExportWrapper.GetMessageAsHtmlAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("1075116548966064128")
        );

        // Assert
        message.Text().Should().Contain("DiscordChatExporter Test Server");
    }
}
