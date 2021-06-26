using System;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public abstract partial class MessageFilter
    {
        public abstract bool Filter(Message message);
    }

    public partial class MessageFilter
    {
        public static MessageFilter CreateFilter(string key, string value)
        {
            return key.ToLower() switch
            {
                "from" => new FromMessageFilter(value),
                "has" => new HasMessageFilter(value),
                "mentions" => new MentionsMessageFilter(value),
                _ => throw new ArgumentException($"Invalid filter type '{key}'.", "key")
            };
        }

        public static MessageFilter? TryParse(string value, IFormatProvider? formatProvider = null)
        {
            throw new NotImplementedException();
        }

        public static MessageFilter Parse(string value, IFormatProvider? formatProvider = null) =>
            TryParse(value, formatProvider) ?? throw new FormatException($"Invalid message filter '{value}'."); ///TODO: better error reporting
    }
}