using System;
using System.Diagnostics.CodeAnalysis;
using Superpower;
using Superpower.Parsers;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class SuperpowerExtensions
{
    extension<T>(TextParser<T> parser)
    {
        public TextParser<T> Token() =>
            parser.Between(Character.WhiteSpace.IgnoreMany(), Character.WhiteSpace.IgnoreMany());

        // Only used for debugging while writing Superpower parsers.
        // From https://twitter.com/nblumhardt/status/1389349059786264578
        [ExcludeFromCodeCoverage]
        public TextParser<T> Log(string description) =>
            i =>
            {
                Console.WriteLine($"Trying {description} ->");
                var r = parser(i);
                Console.WriteLine($"Result was {r}");
                return r;
            };
    }
}
