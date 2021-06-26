using Superpower;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting.Filtering.Parsing
{
    public static class FilterTextParsers
    {
        public static TextParser<string> QuotedString { get; } =
            from open in Character.EqualTo('"')
            from content in Character.EqualTo('\\').IgnoreThen(Character.AnyChar).Try()
                .Or(Character.Except('"'))
                .Many()
            from close in Character.EqualTo('"')
            select new string(content);

        public static TextParser<string> UnquotedString { get; } =
            from content in Character.EqualTo('\\').IgnoreThen(Character.In('"', '/')).Try()
                .Or(Character.Except(c => char.IsWhiteSpace(c) || "():-|\"".Contains(c), "non-whitespace character except for (, ), :, -, |, and \""))
                .AtLeastOnce()
            select new string(content);
    }
}
