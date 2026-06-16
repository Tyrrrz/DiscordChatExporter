using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Infra;
using DiscordChatExporter.Core.Discord;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class JsonEmbedSpecs
{
    [Fact]
    public async Task I_can_export_a_channel_that_contains_a_message_with_a_rich_embed()
    {
        // Act
        var message = await ExportWrapper.GetMessageAsJsonAsync(
            ChannelIds.EmbedTestCases,
            Snowflake.Parse("866769910729146400")
        );

        // Assert
        var embed = message.GetProperty("embeds").EnumerateArray().Single();
        embed.GetProperty("title").GetString().Should().Be("Embed title");
        embed.GetProperty("url").GetString().Should().Be("https://example.com");
        embed.GetProperty("timestamp").GetString().Should().Be("2021-07-14T21:00:00+00:00");
        embed.GetProperty("description").GetString().Should().Be("**Embed** _description_");
        embed.GetProperty("color").GetString().Should().Be("#58B9FF");

        var embedAuthor = embed.GetProperty("author");
        embedAuthor.GetProperty("name").GetString().Should().Be("Embed author");
        embedAuthor.GetProperty("url").GetString().Should().Be("https://example.com/author");
        embedAuthor.GetProperty("iconUrl").GetString().Should().NotBeNullOrWhiteSpace();

        var embedThumbnail = embed.GetProperty("thumbnail");
        embedThumbnail.GetProperty("url").GetString().Should().NotBeNullOrWhiteSpace();
        embedThumbnail.GetProperty("width").GetInt32().Should().Be(120);
        embedThumbnail.GetProperty("height").GetInt32().Should().Be(120);

        var embedFooter = embed.GetProperty("footer");
        embedFooter.GetProperty("text").GetString().Should().Be("Embed footer");
        embedFooter.GetProperty("iconUrl").GetString().Should().NotBeNullOrWhiteSpace();

        var embedFields = embed.GetProperty("fields").EnumerateArray().ToArray();
        embedFields.Should().HaveCount(3);
        embedFields[0].GetProperty("name").GetString().Should().Be("Field 1");
        embedFields[0].GetProperty("value").GetString().Should().Be("Value 1");
        embedFields[0].GetProperty("isInline").GetBoolean().Should().BeTrue();
        embedFields[1].GetProperty("name").GetString().Should().Be("Field 2");
        embedFields[1].GetProperty("value").GetString().Should().Be("Value 2");
        embedFields[1].GetProperty("isInline").GetBoolean().Should().BeTrue();
        embedFields[2].GetProperty("name").GetString().Should().Be("Field 3");
        embedFields[2].GetProperty("value").GetString().Should().Be("Value 3");
        embedFields[2].GetProperty("isInline").GetBoolean().Should().BeTrue();
    }
}
