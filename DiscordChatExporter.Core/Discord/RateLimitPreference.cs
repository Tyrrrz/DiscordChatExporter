using System;

namespace DiscordChatExporter.Core.Discord;

[Flags]
public enum RateLimitPreference
{
    IgnoreAll = 0,
    RespectForUserTokens = 0b1,
    RespectForBotTokens = 0b10,
    RespectAll = RespectForUserTokens | RespectForBotTokens,
}

public static class RateLimitPreferenceExtensions
{
    internal static bool IsRespectedFor(
        this RateLimitPreference rateLimitPreference,
        TokenKind tokenKind
    ) =>
        tokenKind switch
        {
            TokenKind.User => (rateLimitPreference & RateLimitPreference.RespectForUserTokens) != 0,
            TokenKind.Bot => (rateLimitPreference & RateLimitPreference.RespectForBotTokens) != 0,
            _ => throw new ArgumentOutOfRangeException(nameof(tokenKind)),
        };

    public static string GetDisplayName(this RateLimitPreference rateLimitPreference) =>
        rateLimitPreference switch
        {
            RateLimitPreference.IgnoreAll => "Always ignore",
            RateLimitPreference.RespectForUserTokens => "Respect for user tokens",
            RateLimitPreference.RespectForBotTokens => "Respect for bot tokens",
            RateLimitPreference.RespectAll => "Always respect",
            _ => throw new ArgumentOutOfRangeException(nameof(rateLimitPreference)),
        };
}
