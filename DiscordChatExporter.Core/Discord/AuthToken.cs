using System.Net.Http.Headers;

namespace DiscordChatExporter.Core.Discord
{
    public class AuthToken
    {
        public AuthTokenType Type { get; }

        public string Value { get; }

        public AuthToken(AuthTokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public AuthenticationHeaderValue GetAuthenticationHeader() => Type switch
        {
            AuthTokenType.Bot => new AuthenticationHeaderValue("Bot", Value),
            _ => new AuthenticationHeaderValue(Value)
        };

        public override string ToString() => Value;
    }
}