using System.Drawing;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/topics/permissions#role-object
public record Role(Snowflake Id, string Name, int Position, Color? Color) : IHasId
{
    public static Role Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonNullString();
        var position = json.GetProperty("position").GetInt32();

        var color = json.GetPropertyOrNull("color")
            ?.GetInt32OrNull()
            ?.Pipe(System.Drawing.Color.FromArgb)
            .ResetAlpha()
            .NullIf(c => c.ToRgb() <= 0);

        return new Role(id, name, position, color);
    }
}
