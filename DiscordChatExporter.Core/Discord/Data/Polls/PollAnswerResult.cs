using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Polls;

// https://discord.com/developers/docs/resources/poll#poll-answer-count-object
public record PollAnswerResult(int AnswerId, int Count, bool DidCurrentUserVote)
{
    public static PollAnswerResult Parse(JsonElement json)
    {
        var answerId = json.GetProperty("id").GetInt32();
        var count = json.GetProperty("count").GetInt32();
        var didCurrentUserVote = json.GetPropertyOrNull("me_voted")?.GetBooleanOrNull() ?? false;

        return new PollAnswerResult(answerId, count, didCurrentUserVote);
    }
}
