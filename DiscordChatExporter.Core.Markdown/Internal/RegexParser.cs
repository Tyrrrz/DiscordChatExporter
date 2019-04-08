using System;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    public class RegexParser<T>
    {
        public Regex Regex { get; }
        public Func<Match, T> Transform { get; }

        public RegexParser(Regex regex, Func<Match, T> transform)
        {
            Regex = regex;
            Transform = transform;
        }
    }
}