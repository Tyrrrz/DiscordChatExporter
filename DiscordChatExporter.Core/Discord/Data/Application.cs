using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/application#application-object
public partial record Application(Snowflake Id, string Name, ApplicationFlags Flags)
{
    public bool IsMessageContentIntentEnabled =>
        Flags.HasFlag(ApplicationFlags.GatewayMessageContent)
        || Flags.HasFlag(ApplicationFlags.GatewayMessageContentLimited);
}

public partial record Application
{
    public static Application Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonWhiteSpaceString();

        var flags =
            json.GetPropertyOrNull("flags")?.GetInt32OrNull()?.Pipe(x => (ApplicationFlags)x)
            ?? ApplicationFlags.None;

        return new Application(id, name, flags);
    }
}
