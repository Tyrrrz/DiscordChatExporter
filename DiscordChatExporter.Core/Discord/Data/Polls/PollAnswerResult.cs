using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Polls;

// https://discord.com/developers/docs/resources/poll#poll-answer-count-object
public record PollAnswerResult(int Id, int Count, bool DidCurrentUserVote)
{
    public static PollAnswerResult Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetInt32();
        var count = json.GetProperty("count").GetInt32();
        var didCurrentUserVote = json.GetPropertyOrNull("me_voted")?.GetBooleanOrNull() ?? false;

        return new PollAnswerResult(id, count, didCurrentUserVote);
    }
}
