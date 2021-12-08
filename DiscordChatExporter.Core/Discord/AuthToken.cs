using System.Net.Http.Headers;

namespace DiscordChatExporter.Core.Discord;

public record AuthToken(AuthTokenKind Kind, string Value)
{
    public AuthenticationHeaderValue GetAuthenticationHeader() => Kind switch
    {
        AuthTokenKind.Bot => new AuthenticationHeaderValue("Bot", Value),
        _ => new AuthenticationHeaderValue(Value)
    };
}