using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordChatExporter.Cli.Tests.Utils;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using FluentAssertions;
using Xunit;

namespace DiscordChatExporter.Cli.Tests.Specs;

public class HtmlPollSpecs
{
    private static Message ParsePollMessage()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "id": "1503394223213383801",
              "type": 0,
              "author": {
                "id": "1503390786950135868",
                "username": "poll-author",
                "global_name": "Poll Author",
                "avatar": null
              },
              "timestamp": "2026-05-11T14:00:00+00:00",
              "content": "",
              "attachments": [],
              "embeds": [],
              "sticker_items": [],
              "reactions": [],
              "mentions": [],
              "poll": {
                "question": { "text": "What <should> we eat?" },
                "answers": [
                  {
                    "answer_id": 1,
                    "poll_media": { "text": "Pizza & pasta", "emoji": { "id": null, "name": "🍕" } }
                  },
                  {
                    "answer_id": 5,
                    "poll_media": { "text": "Tacos", "emoji": { "id": "1503391152383070351", "name": "taco", "animated": false } }
                  },
                  {
                    "answer_id": 9,
                    "poll_media": { "text": "Salad" }
                  }
                ],
                "expiry": "2026-05-12T14:00:00+00:00",
                "allow_multiselect": true,
                "layout_type": 1,
                "results": {
                  "is_finalized": true,
                  "answer_counts": [
                    { "id": 1, "count": 3, "me_voted": false },
                    { "id": 5, "count": 2, "me_voted": true }
                  ]
                }
              }
            }
            """
        );

        return Message.Parse(document.RootElement);
    }

    [Fact]
    public void I_can_parse_a_poll_only_message()
    {
        // Act
        var message = ParsePollMessage();

        // Assert
        message.IsEmpty.Should().BeFalse();
        message.Poll.Should().NotBeNull();

        var poll = message.Poll!;
        poll.Question.Should().Be("What <should> we eat?");
        poll.Answers.Select(a => a.Id).Should().Equal(1, 5, 9);
        poll.Answers[1].Emoji.Should().NotBeNull();
        poll.AllowsMultipleAnswers.Should().BeTrue();
        poll.Results.Should().NotBeNull();
        poll.Results!.IsFinalized.Should().BeTrue();
        poll.Results.TotalVoteCount.Should().Be(5);
        poll.Results.GetAnswerCount(5).DidCurrentUserVote.Should().BeTrue();
        poll.Results.GetAnswerCount(9).Count.Should().Be(0);
    }

    [Fact]
    public void I_can_parse_a_poll_without_results()
    {
        // Arrange
        using var document = JsonDocument.Parse(
            """
            {
              "question": { "text": "Still voting?" },
              "answers": [
                { "answer_id": 42, "poll_media": { "text": "Yes" } }
              ],
              "expiry": null,
              "allow_multiselect": false,
              "layout_type": 1
            }
            """
        );

        // Act
        var poll = Poll.Parse(document.RootElement);

        // Assert
        poll.Results.Should().BeNull();
        poll.ExpiresAt.Should().BeNull();
        poll.Answers.Should().ContainSingle().Which.Id.Should().Be(42);
    }

    [Fact]
    public async Task I_can_render_a_poll_in_the_HTML_format()
    {
        // Arrange
        var message = ParsePollMessage();
        var guild = new Guild(new Snowflake(1), "Guild", "");
        var channel = new Channel(
            new Snowflake(2),
            ChannelKind.GuildTextChat,
            guild.Id,
            null,
            "polls",
            0,
            null,
            null,
            false,
            message.Id
        );
        var request = new ExportRequest(
            guild,
            channel,
            Path.Combine(Path.GetTempPath(), "poll.html"),
            null,
            ExportFormat.HtmlDark,
            null,
            null,
            PartitionLimit.Null,
            MessageFilter.Null,
            false,
            true,
            false,
            false,
            "en-US",
            true
        );
        var context = new ExportContext(new DiscordClient("unused"), request);

        // Act
        var html = await new MessageGroupTemplate
        {
            Context = context,
            Messages = [message],
        }.RenderAsync();
        var document = Html.Parse(html);
        var poll = document.QuerySelector(".chatlog__poll");

        // Assert
        poll.Should().NotBeNull();
        poll!
            .QuerySelector(".chatlog__poll-question")!
            .TextContent.Should()
            .Be("What <should> we eat?");
        poll.QuerySelector("should").Should().BeNull();
        poll.QuerySelectorAll(".chatlog__poll-answer").Should().HaveCount(3);
        poll.QuerySelectorAll(".chatlog__poll-answer-text")
            .Select(e => e.TextContent)
            .Should()
            .Equal("Pizza & pasta", "Tacos", "Salad");
        poll.QuerySelectorAll(".chatlog__poll-answer-count")
            .Select(e => e.TextContent)
            .Should()
            .Equal("3", "2", "0");
        poll.QuerySelector(".chatlog__poll-answer--selected .chatlog__poll-answer-text")!
            .TextContent.Should()
            .Be("Tacos");
        poll.QuerySelector(".chatlog__poll-answer-emoji[alt='🍕']").Should().NotBeNull();
        poll.QuerySelector(".chatlog__poll-footer")!
            .TextContent.Should()
            .ContainAll("5 votes", "Multiple answers allowed", "Final results", "Ended");
    }
}
