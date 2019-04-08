using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    public class RegexAggregator<T>
    {
        private readonly IReadOnlyList<RegexParser<T>> _parsers;

        public RegexAggregator(IReadOnlyList<RegexParser<T>> parsers)
        {
            _parsers = parsers;
        }

        public RegexAggregator(params RegexParser<T>[] parsers)
            : this((IReadOnlyList<RegexParser<T>>)parsers)
        {
        }

        public IReadOnlyList<T> Parse(string input)
        {
            var matches = new List<TransformedMatch<T>>();

            // Loop through each parser and process parts of input that are not yet covered by matches
            foreach (var parser in _parsers)
            {
                var startIndex = 0;

                // Process ranges between 0 and first match, first match and second match, etc
                foreach (var match in matches.Select(i => i.Match).ToArray())
                {
                    // Get length since start index
                    var length = match.Index - startIndex;

                    // Get matches for this segment
                    var newMatches = parser.Regex.Matches(input, startIndex, length)
                        .Select(m => new TransformedMatch<T>(m, parser.Transform(m)));

                    // Add matches to list
                    matches.AddRange(newMatches);

                    // Offset start index
                    startIndex = match.Index + match.Length;
                }

                // Process range between last match and EOL
                if (startIndex < input.Length)
                {
                    // Get matches for this segment
                    var newMatches = parser.Regex.Matches(input, startIndex)
                        .Cast<Match>()
                        .Select(m => new TransformedMatch<T>(m, parser.Transform(m)));

                    // Add matches to list
                    matches.AddRange(newMatches);
                }

                // Sort matches by index
                matches.Sort(m => m.Match.Index);
            }

            return matches.Select(i => i.Value).ToArray();
        }
    }
}