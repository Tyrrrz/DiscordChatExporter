using System;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Filtering.Parsing;
using Superpower;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public abstract partial class MessageFilter
    {
        public abstract bool Filter(Message message);
    }

    public partial class MessageFilter
    {
        protected const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline;

        public static MessageFilter CreateFilter(string text) => new ContainsMessageFilter(text);

        public static MessageFilter CreateFilter(string key, string value)
        {
            return key.ToLowerInvariant() switch
            {
                "from" => new FromMessageFilter(value),
                "has" => new HasMessageFilter(value),
                "mentions" => new MentionsMessageFilter(value),
                _ => throw new ArgumentException($"Invalid filter type '{key}'.", nameof(key))
            };
        }

        public static MessageFilter Parse(string value, IFormatProvider? formatProvider = null)
        {
            var tokens = FilterTokenizer.Instance.Tokenize(value);
            var parsed = FilterParser.Instance.Parse(tokens);
            return parsed;
        }
    }
}
