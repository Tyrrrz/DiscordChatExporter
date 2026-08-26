using System;
using System.Globalization;
using System.Linq;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://discord.com/developers/docs/resources/message#poll-result-embed-fields
public record PollResultEmbedProjection(
    string QuestionText,
    int WinningVoteCount,
    int TotalVoteCount,
    int? WinningAnswerId,
    string? WinningAnswerText,
    Emoji? WinningAnswerEmoji
)
{
    public double WinningVotePercentage { get; } =
        TotalVoteCount > 0 ? (double)WinningVoteCount / TotalVoteCount : 0;

    private static string? TryGetFieldValue(Embed embed, string name) =>
        embed
            .Fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.Ordinal))
            ?.Value;

    private static int ParseInt32OrDefault(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;

    private static Emoji? TryParseWinningAnswerEmoji(Embed embed)
    {
        var name = TryGetFieldValue(embed, "victor_answer_emoji_name");
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var id =
            TryGetFieldValue(embed, "victor_answer_emoji_id") is { } idValue
            && Snowflake.TryParse(idValue) is { } parsedId
                ? parsedId
                : (Snowflake?)null;

        var isAnimated =
            bool.TryParse(
                TryGetFieldValue(embed, "victor_answer_emoji_animated"),
                out var parsedIsAnimated
            ) && parsedIsAnimated;

        return new Emoji(id, name, isAnimated);
    }

    public static PollResultEmbedProjection? TryResolve(Embed embed)
    {
        if (embed.Kind != EmbedKind.PollResult)
            return null;

        var questionText = TryGetFieldValue(embed, "poll_question_text") ?? "";
        var winningVoteCount = ParseInt32OrDefault(TryGetFieldValue(embed, "victor_answer_votes"));
        var totalVoteCount = ParseInt32OrDefault(TryGetFieldValue(embed, "total_votes"));

        var winningAnswerId = int.TryParse(
            TryGetFieldValue(embed, "victor_answer_id"),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var parsedWinningAnswerId
        )
            ? parsedWinningAnswerId
            : (int?)null;

        var winningAnswerText = TryGetFieldValue(embed, "victor_answer_text");
        var winningAnswerEmoji = TryParseWinningAnswerEmoji(embed);

        return new PollResultEmbedProjection(
            questionText,
            winningVoteCount,
            totalVoteCount,
            winningAnswerId,
            winningAnswerText,
            winningAnswerEmoji
        );
    }
}
