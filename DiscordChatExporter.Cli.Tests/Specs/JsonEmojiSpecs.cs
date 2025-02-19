using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class JsonEmojiSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_inline_emoji_and_have_them_listed_separately()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.EmojiTestCases,
            Snowflake.Parse("866768521052553216")
        );

        // Assert
        var inlineEmojis = message.GetProperty("inlineEmojis").EnumerateArray().ToArray();
        inlineEmojis.Should().HaveCount(4);

        inlineEmojis[0].GetProperty("id").GetString().Should().BeNullOrEmpty();
        inlineEmojis[0].GetProperty("name").GetString().Should().Be("🙂");
        inlineEmojis[0].GetProperty("code").GetString().Should().Be("slight_smile");
        inlineEmojis[0].GetProperty("isAnimated").GetBoolean().Should().BeFalse();
        inlineEmojis[0].GetProperty("imageUrl").GetString().Should().NotBeNullOrWhiteSpace();

        inlineEmojis[1].GetProperty("id").GetString().Should().BeNullOrEmpty();
        inlineEmojis[1].GetProperty("name").GetString().Should().Be("😦");
        inlineEmojis[1].GetProperty("code").GetString().Should().Be("frowning");
        inlineEmojis[1].GetProperty("isAnimated").GetBoolean().Should().BeFalse();
        inlineEmojis[1].GetProperty("imageUrl").GetString().Should().NotBeNullOrWhiteSpace();

        inlineEmojis[2].GetProperty("id").GetString().Should().BeNullOrEmpty();
        inlineEmojis[2].GetProperty("name").GetString().Should().Be("😔");
        inlineEmojis[2].GetProperty("code").GetString().Should().Be("pensive");
        inlineEmojis[2].GetProperty("isAnimated").GetBoolean().Should().BeFalse();
        inlineEmojis[2].GetProperty("imageUrl").GetString().Should().NotBeNullOrWhiteSpace();

        inlineEmojis[3].GetProperty("id").GetString().Should().BeNullOrEmpty();
        inlineEmojis[3].GetProperty("name").GetString().Should().Be("😂");
        inlineEmojis[3].GetProperty("code").GetString().Should().Be("joy");
        inlineEmojis[3].GetProperty("isAnimated").GetBoolean().Should().BeFalse();
        inlineEmojis[3].GetProperty("imageUrl").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_custom_inline_emoji_and_have_them_listed_separately()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.EmojiTestCases,
            Snowflake.Parse("1299804867447230594")
        );

        // Assert
        var inlineEmojis = message.GetProperty("inlineEmojis").EnumerateArray().ToArray();
        inlineEmojis.Should().HaveCount(1);

        inlineEmojis[0].GetProperty("id").GetString().Should().Be("754441880066064584");
        inlineEmojis[0].GetProperty("name").GetString().Should().Be("lemon_blush");
        inlineEmojis[0].GetProperty("code").GetString().Should().Be("lemon_blush");
        inlineEmojis[0].GetProperty("isAnimated").GetBoolean().Should().BeFalse();
        inlineEmojis[0].GetProperty("imageUrl").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
