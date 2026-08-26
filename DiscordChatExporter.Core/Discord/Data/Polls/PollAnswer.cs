using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data;
using JsonExtensions.Reading;
using PowerKit.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Polls;

// https://discord.com/developers/docs/resources/poll#poll-answer-object
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
