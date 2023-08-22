using System.Text.Json;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/invite#invite-object
public record Invite(string Code, Guild Guild, Channel? Channel)
{
    public static string? TryGetCodeFromUrl(string url) =>
        Regex.Match(url, @"^https?://discord\.gg/(\w+)/?$").Groups[1].Value.NullIfWhiteSpace();

    public static Invite Parse(JsonElement json)
    {
        var code = json.GetProperty("code").GetNonWhiteSpaceString();
        var guild = json.GetPropertyOrNull("guild")?.Pipe(Guild.Parse) ?? Guild.DirectMessages;
        var channel = json.GetPropertyOrNull("channel")?.Pipe(c => Channel.Parse(c));

        return new Invite(code, guild, channel);
    }
}
