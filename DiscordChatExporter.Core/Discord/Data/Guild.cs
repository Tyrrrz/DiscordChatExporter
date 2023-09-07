using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/guild#guild-object
public partial record Guild(Snowflake Id, string Name, string IconUrl) : IHasId
{
    public bool IsDirect => Id == DirectMessages.Id;
}

public partial record Guild
{
    // Direct messages are encapsulated within a special pseudo-guild for consistency
    public static Guild DirectMessages { get; } =
        new(Snowflake.Zero, "Direct Messages", ImageCdn.GetFallbackUserAvatarUrl());

    public static Guild Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonNullString();

        var iconUrl =
            json.GetPropertyOrNull("icon")
                ?.GetNonWhiteSpaceStringOrNull()
                ?.Pipe(h => ImageCdn.GetGuildIconUrl(id, h)) ?? ImageCdn.GetFallbackUserAvatarUrl();

        return new Guild(id, name, iconUrl);
    }
}
