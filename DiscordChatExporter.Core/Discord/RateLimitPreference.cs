using System;

namespace DiscordChatExporter.Core.Discord;

[Flags]
public enum RateLimitPreference
{
    IgnoreAll = 0,
    RespectForUserRequests = 0b1,
    RespectForBotRequests = 0b10,
    RespectAll = RespectForUserRequests | RespectForBotRequests,
}

public static class RateLimitPreferenceExtensions
{
    internal static bool ShouldRespect(
        this RateLimitPreference rateLimitPreference,
        TokenKind tokenKind
    ) =>
        tokenKind == TokenKind.Bot
            ? (rateLimitPreference & RateLimitPreference.RespectForBotRequests) != 0
            : (rateLimitPreference & RateLimitPreference.RespectForUserRequests) != 0;

    public static string GetDisplayName(this RateLimitPreference rateLimitPreference) =>
        rateLimitPreference switch
        {
            RateLimitPreference.IgnoreAll => "Always ignore",
            RateLimitPreference.RespectForUserRequests => "Respect for user requests",
            RateLimitPreference.RespectForBotRequests => "Respect for bot requests",
            RateLimitPreference.RespectAll => "Always respect",
            _ => throw new ArgumentOutOfRangeException(nameof(rateLimitPreference)),
        };
}
