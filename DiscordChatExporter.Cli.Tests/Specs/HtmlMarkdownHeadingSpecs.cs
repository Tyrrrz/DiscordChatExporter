using System;
using System.IO;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlMarkdownHeadingSpecs
{
    private static async ValueTask<string> FormatMarkdownAsync(string markdown) =>
        await HtmlMarkdownVisitor.FormatAsync(null!, markdown);

    private static ExportContext CreateExportContext(bool shouldFormatMarkdown)
    {
        var guild = Guild.DirectMessages;
        var channel = new Channel(
            new Snowflake(1),
            ChannelKind.DirectTextChat,
            guild.Id,
            null,
            "test",
            null,
            null,
            null,
            false,
            null
        );

        var request = new ExportRequest(
            guild,
            channel,
            Path.Combine(Path.GetTempPath(), "DiscordChatExporter.Tests", "test.html"),
            null,
            ExportFormat.HtmlDark,
            null,
            null,
            PartitionLimit.Null,
            MessageFilter.Null,
            false,
            shouldFormatMarkdown,
            false,
            true,
            "en-US",
            true
        );

        return new ExportContext(new DiscordClient(""), request);
    }

    private static Message CreateMessage(string content) =>
        new(
            new Snowflake(2),
            MessageKind.Default,
            MessageFlags.None,
            new User(new Snowflake(3), false, 1234, "test-user", "Test User", ""),
            DateTimeOffset.Parse("2024-01-01T00:00:00+00:00"),
            null,
            null,
            false,
            content,
            [],
            [],
            [],
            [],
            [],
            null,
            null,
            null,
            null
        );

    [Theory]
    [InlineData("# Heading", "<h1>Heading</h1>")]
    [InlineData("# Heading\nbody", "<h1>Heading</h1>body")]
    [InlineData("# Heading\r\nbody", "<h1>Heading</h1>body")]
    [InlineData("## Heading", "<h2>Heading</h2>")]
    [InlineData("### Heading", "<h3>Heading</h3>")]
    public async Task I_can_render_markdown_headings_as_html(string markdown, string expectedHtml)
    {
        // Act
        var html = await FormatMarkdownAsync(markdown);

        // Assert
        html.Should().Be(expectedHtml);
    }

    [Theory]
    [InlineData("#hashtag")]
    [InlineData("#")]
    [InlineData("#\nHeading")]
    [InlineData("#\r\nHeading")]
    public async Task I_do_not_render_non_heading_hash_text_as_html_heading(string markdown)
    {
        // Act
        var html = await FormatMarkdownAsync(markdown);

        // Assert
        html.Should().Be(markdown);
        html.Should().NotContain("<h");
    }

    [Fact]
    public async Task I_can_escape_html_sensitive_text_inside_markdown_headings()
    {
        // Act
        var html = await FormatMarkdownAsync("# <Heading> & \"quote\"");

        // Assert
        html.Should().Be("<h1>&lt;Heading&gt; &amp; &quot;quote&quot;</h1>");
    }

    [Fact]
    public async Task I_keep_markdown_headings_literal_when_formatting_is_disabled()
    {
        // Arrange
        var context = CreateExportContext(false);
        var message = CreateMessage("# <Heading> & \"quote\"");

        // Act
        var html = await new MessageGroupTemplate
        {
            Context = context,
            Messages = [message],
        }.RenderAsync();

        // Assert
        html.Should().Contain("# &lt;Heading&gt; &amp;");
        html.Should().NotContain("# <Heading>");
        html.Should().NotContain("<h1>");
    }
}
