using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Polls;

// https://discord.com/developers/docs/resources/poll#poll-results-object
public record PollResults(bool IsFinalized, IReadOnlyList<PollAnswerResult> Answers)
{
    public int TotalVoteCount { get; } = Answers.Sum(a => a.Count);

    public int WinningVoteCount { get; } = Answers.Select(a => a.Count).DefaultIfEmpty().Max();

    public PollAnswerResult? TryGetAnswerResult(int answerId) =>
        Answers.FirstOrDefault(a => a.AnswerId == answerId);

    public static PollResults Parse(JsonElement json)
    {
        var isFinalized = json.GetPropertyOrNull("is_finalized")?.GetBooleanOrNull() ?? false;

        var answers =
            json.GetPropertyOrNull("answer_counts")
                ?.EnumerateArrayOrNull()
                ?.Select(PollAnswerResult.Parse)
                .ToArray()
            ?? [];

        return new PollResults(isFinalized, answers);
    }
}
