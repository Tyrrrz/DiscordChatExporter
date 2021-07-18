using System.Net.Http.Headers;

namespace DiscordChatExporter.Core.Discord
{
    public class AuthToken
    {
        public AuthTokenKind Kind { get; }

        public string Value { get; }

        public AuthToken(AuthTokenKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        public AuthenticationHeaderValue GetAuthenticationHeader() => Kind switch
        {
            AuthTokenKind.Bot => new AuthenticationHeaderValue("Bot", Value),
            _ => new AuthenticationHeaderValue(Value)
        };

        public override string ToString() => Value;
    }
}