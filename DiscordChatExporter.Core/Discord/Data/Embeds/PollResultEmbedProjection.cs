using System.Globalization;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Embeds;

// https://docs.discord.com/developers/resources/message#embed-fields-by-embed-type-poll-result-embed-fields
public partial record PollResultEmbedProjection(
    string Question,
    int TotalVoteCount,
    int WinningVoteCount,
    int? WinningAnswerId,
    string? WinningAnswerText,
    Emoji? WinningAnswerEmoji
)
{
    public double WinningVoteShare { get; } =
        TotalVoteCount > 0 ? (double)WinningVoteCount / TotalVoteCount : 0;
}

public partial record PollResultEmbedProjection
{
    private static Emoji? TryParseWinningAnswerEmoji(Embed embed)
    {
        var name = embed.TryGetField("victor_answer_emoji_name")?.Value;
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var id = embed
            .TryGetField("victor_answer_emoji_id")
            ?.Value?.Pipe(v => Snowflake.TryParse(v));

        var isAnimated =
            embed.TryGetField("victor_answer_emoji_animated")?.Value?.Pipe(bool.ParseOrNull)
            ?? false;

        return new Emoji(id, name, isAnimated);
    }

    public static PollResultEmbedProjection? TryResolve(Embed embed)
    {
        if (embed.Kind != EmbedKind.PollResult)
            return null;

        var question = embed.TryGetField("poll_question_text")?.Value ?? "";

        var totalVoteCount =
            embed
                .TryGetField("total_votes")
                ?.Value?.Pipe(v => int.ParseOrNull(v, CultureInfo.InvariantCulture))
            ?? 0;

        var winningVoteCount =
            embed
                .TryGetField("victor_answer_votes")
                ?.Value?.Pipe(v => int.ParseOrNull(v, CultureInfo.InvariantCulture))
            ?? 0;

        var winningAnswerId = embed
            .TryGetField("victor_answer_id")
            ?.Value?.Pipe(v => int.ParseOrNull(v, CultureInfo.InvariantCulture));

        var winningAnswerText = embed.TryGetField("victor_answer_text")?.Value;
        var winningAnswerEmoji = TryParseWinningAnswerEmoji(embed);

        return new PollResultEmbedProjection(
            question,
            totalVoteCount,
            winningVoteCount,
            winningAnswerId,
            winningAnswerText,
            winningAnswerEmoji
        );
    }
}
