using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JsonExtensions.Reading;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Polls;

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
