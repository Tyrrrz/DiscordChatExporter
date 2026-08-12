using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JsonExtensions.Reading;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/poll#poll-object
public record Poll(
    string Question,
    IReadOnlyList<PollAnswer> Answers,
    DateTimeOffset? ExpiresAt,
    bool AllowsMultipleAnswers,
    PollResults? Results
)
{
    public static Poll Parse(JsonElement json)
    {
        var question =
            json.GetProperty("question").GetPropertyOrNull("text")?.GetStringOrNull() ?? "";

        var answers =
            json.GetPropertyOrNull("answers")
                ?.EnumerateArrayOrNull()
                ?.Select(PollAnswer.Parse)
                .ToArray()
            ?? [];

        var expiresAt = json.GetPropertyOrNull("expiry")?.GetDateTimeOffsetOrNull();
        var allowsMultipleAnswers =
            json.GetPropertyOrNull("allow_multiselect")?.GetBooleanOrNull() ?? false;
        var results = json.GetPropertyOrNull("results")?.Pipe(PollResults.Parse);

        return new Poll(question, answers, expiresAt, allowsMultipleAnswers, results);
    }
}

public record PollAnswer(int Id, string Text, Emoji? Emoji)
{
    public static PollAnswer Parse(JsonElement json)
    {
        var id = json.GetProperty("answer_id").GetInt32();
        var media = json.GetProperty("poll_media");
        var text = media.GetPropertyOrNull("text")?.GetStringOrNull() ?? "";
        var emoji = media.GetPropertyOrNull("emoji")?.Pipe(Emoji.Parse);

        return new PollAnswer(id, text, emoji);
    }
}

public record PollAnswerCount(int AnswerId, int Count, bool DidCurrentUserVote)
{
    public static PollAnswerCount Parse(JsonElement json)
    {
        var answerId = json.GetProperty("id").GetInt32();
        var count = json.GetProperty("count").GetInt32();
        var didCurrentUserVote = json.GetPropertyOrNull("me_voted")?.GetBooleanOrNull() ?? false;

        return new PollAnswerCount(answerId, count, didCurrentUserVote);
    }
}

public record PollResults(bool IsFinalized, IReadOnlyDictionary<int, PollAnswerCount> AnswerCounts)
{
    public int TotalVoteCount { get; } = AnswerCounts.Values.Sum(c => c.Count);

    public PollAnswerCount GetAnswerCount(int answerId) =>
        AnswerCounts.GetValueOrDefault(answerId) ?? new PollAnswerCount(answerId, 0, false);

    public static PollResults Parse(JsonElement json)
    {
        var isFinalized = json.GetPropertyOrNull("is_finalized")?.GetBooleanOrNull() ?? false;

        var answerCounts =
            json.GetPropertyOrNull("answer_counts")
                ?.EnumerateArrayOrNull()
                ?.Select(PollAnswerCount.Parse)
                .ToDictionary(c => c.AnswerId)
            ?? [];

        return new PollResults(isFinalized, answerCounts);
    }
}
