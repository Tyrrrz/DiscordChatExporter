using System;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/user#user-object
public partial record User(
    Snowflake Id,
    bool IsBot,
    int Discriminator,
    string Name,
    string AvatarUrl) : IHasId
{
    public string DiscriminatorFormatted => $"{Discriminator:0000}";

    public string FullName => $"{Name}#{DiscriminatorFormatted}";
}

public partial record User
{
    private static string GetDefaultAvatarUrl(int discriminator) =>
        $"https://cdn.discordapp.com/embed/avatars/{discriminator % 5}.png";

    private static string GetAvatarUrl(Snowflake id, string avatarHash)
    {
        var extension = avatarHash.StartsWith("a_", StringComparison.Ordinal)
            ? "gif"
            : "png";

        return $"https://cdn.discordapp.com/avatars/{id}/{avatarHash}.{extension}?size=128";
    }

    public static User Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var isBot = json.GetPropertyOrNull("bot")?.GetBooleanOrNull() ?? false;
        var discriminator = json.GetProperty("discriminator").GetNonWhiteSpaceString().Pipe(int.Parse);
        var name = json.GetProperty("username").GetNonNullString();
        var avatarHash = json.GetPropertyOrNull("avatar")?.GetNonWhiteSpaceStringOrNull();

        var avatarUrl = !string.IsNullOrWhiteSpace(avatarHash)
            ? GetAvatarUrl(id, avatarHash)
            : GetDefaultAvatarUrl(discriminator);

        return new User(id, isBot, discriminator, name, avatarUrl);
    }
}