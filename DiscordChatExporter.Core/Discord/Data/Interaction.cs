using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/interactions/receiving-and-responding#message-interaction-object
public record Interaction(Snowflake Id, string Name, User User)
{
    public static Interaction Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonNullString(); // may be empty, but not null
        var user = json.GetProperty("user").Pipe(User.Parse);

        return new Interaction(id, name, user);
    }
}
