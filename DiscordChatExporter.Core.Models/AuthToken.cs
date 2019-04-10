namespace DiscordChatExporter.Core.Models
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
    }
}