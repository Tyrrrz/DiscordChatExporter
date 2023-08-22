using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/user#user-object
public partial record User(
    Snowflake Id,
    bool IsBot,
    // Remove after Discord migrates all accounts to the new system.
    // With that, also remove the DiscriminatorFormatted and FullName properties.
    // Replace existing calls to FullName with Name (not DisplayName).
    int? Discriminator,
    string Name,
    string DisplayName,
    string AvatarUrl
) : IHasId
{
    public string DiscriminatorFormatted =>
        Discriminator is not null ? $"{Discriminator:0000}" : "0000";

    // This effectively represents the user's true identity.
    // In the old system, this is formed from the username and discriminator.
    // In the new system, the username is already the user's unique identifier.
    public string FullName => Discriminator is not null ? $"{Name}#{DiscriminatorFormatted}" : Name;
}

public partial record User
{
    public static User Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var isBot = json.GetPropertyOrNull("bot")?.GetBooleanOrNull() ?? false;

        var discriminator = json.GetPropertyOrNull("discriminator")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(int.Parse)
            .NullIfDefault();

        var name = json.GetProperty("username").GetNonNullString();
        var displayName =
            json.GetPropertyOrNull("global_name")?.GetNonWhiteSpaceStringOrNull() ?? name;

        var avatarIndex = discriminator % 5 ?? (int)((id.Value >> 22) % 6);

        var avatarUrl =
            json.GetPropertyOrNull("avatar")
                ?.GetNonWhiteSpaceStringOrNull()
                ?.Pipe(h => ImageCdn.GetUserAvatarUrl(id, h))
            ?? ImageCdn.GetFallbackUserAvatarUrl(avatarIndex);

        return new User(id, isBot, discriminator, name, displayName, avatarUrl);
    }
}
