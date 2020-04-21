using System.Net.Http.Headers;

namespace DiscordChatExporter.Domain.Discord
{
    public enum AuthTokenType
    {
        User,
        Bot
    }

    public class AuthToken
    {
        public AuthTokenType Type { get; }

        public string Value { get; }

        public AuthToken(AuthTokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public AuthenticationHeaderValue GetAuthenticationHeader() => Type == AuthTokenType.User
            ? new AuthenticationHeaderValue(Value)
            : new AuthenticationHeaderValue("Bot", Value);

        public override string ToString() => Value;
    }
}