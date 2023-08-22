using System;
using System.Diagnostics.CodeAnalysis;
using Superpower;
using Superpower.Parsers;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class SuperpowerExtensions
{
    public static TextParser<string> Text(this TextParser<char[]> parser) =>
        parser.Select(chars => new string(chars));

    public static TextParser<T> Token<T>(this TextParser<T> parser) =>
        parser.Between(Character.WhiteSpace.IgnoreMany(), Character.WhiteSpace.IgnoreMany());

    // Only used for debugging while writing Superpower parsers.
    // From https://twitter.com/nblumhardt/status/1389349059786264578
    [ExcludeFromCodeCoverage]
    public static TextParser<T> Log<T>(this TextParser<T> parser, string description) =>
        i =>
        {
            Console.WriteLine($"Trying {description} ->");
            var r = parser(i);
            Console.WriteLine($"Result was {r}");
            return r;
        };
}
