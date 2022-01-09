using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/guild#guild-object
public record Guild(Snowflake Id, string Name, string IconUrl) : IHasId
{
    public static Guild DirectMessages { get; } = new(
        Snowflake.Zero,
        "Direct Messages",
        GetDefaultIconUrl()
    );

    private static string GetDefaultIconUrl() =>
        "https://cdn.discordapp.com/embed/avatars/0.png";

    private static string GetIconUrl(Snowflake id, string iconHash) =>
        $"https://cdn.discordapp.com/icons/{id}/{iconHash}.png";

    public static Guild Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonNullString();
        var iconHash = json.GetPropertyOrNull("icon")?.GetNonWhiteSpaceStringOrNull();

        var iconUrl = !string.IsNullOrWhiteSpace(iconHash)
            ? GetIconUrl(id, iconHash)
            : GetDefaultIconUrl();

        return new Guild(id, name, iconUrl);
    }
}