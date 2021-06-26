using System;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Filtering.Parsing;
using Superpower;
using Superpower.Model;

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
                "contains" => new ContainsMessageFilter(value),
                "from" => new FromMessageFilter(value),
                "has" => new HasMessageFilter(value),
                "mentions" => new MentionsMessageFilter(value),
                _ => throw new ArgumentException($"Invalid filter type '{key}'.", "key")
            };
        }

        public static MessageFilter Parse(string value, IFormatProvider? formatProvider = null) =>
            FilterParser.TryParse(value, out var filter, out var error, out var position) ? filter : throw new ParseException(error, position);

    }
}