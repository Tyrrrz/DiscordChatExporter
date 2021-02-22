using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Matching
{
    internal class AggregateMatcher<T> : IMatcher<T>
    {
        private readonly IReadOnlyList<IMatcher<T>> _matchers;

        public AggregateMatcher(IReadOnlyList<IMatcher<T>> matchers)
        {
            _matchers = matchers;
        }

        public AggregateMatcher(params IMatcher<T>[] matchers)
            : this((IReadOnlyList<IMatcher<T>>) matchers)
        {
        }

        public ParsedMatch<T>? TryMatch(StringPart stringPart)
        {
            ParsedMatch<T>? earliestMatch = null;

            // Try to match the input with each matcher and get the match with the lowest start index
            foreach (var matcher in _matchers)
            {
                // Try to match
                var match = matcher.TryMatch(stringPart);

                // If there's no match - continue
                if (match == null)
                    continue;

                // If this match is earlier than previous earliest - replace
                if (earliestMatch == null || match.StringPart.StartIndex < earliestMatch.StringPart.StartIndex)
                    earliestMatch = match;

                // If the earliest match starts at the very beginning - break,
                // because it's impossible to find a match earlier than that
                if (earliestMatch.StringPart.StartIndex == stringPart.StartIndex)
                    break;
            }

            return earliestMatch;
        }
    }
}