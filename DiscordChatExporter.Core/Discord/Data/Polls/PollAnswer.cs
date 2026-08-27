using System.Text.Json;
using JsonExtensions.Reading;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Polls;

// https://discord.com/developers/docs/resources/poll#poll-answer-object
public record PollAnswer(int Id, string Text, Emoji? Emoji)
{
    public static PollAnswer Parse(JsonElement json)
    {
        var id = json.GetProperty("answer_id").GetInt32();

        var text =
            json.GetPropertyOrNull("poll_media")?.GetPropertyOrNull("text")?.GetStringOrNull()
            ?? "";

        var emoji = json.GetPropertyOrNull("poll_media")
            ?.GetPropertyOrNull("emoji")
            ?.Pipe(Emoji.Parse);

        return new PollAnswer(id, text, emoji);
    }
}
