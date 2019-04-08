using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Internal
{
    internal class AggregatedParser<T> : IParser<T>
    {
        private readonly IReadOnlyList<IParser<T>> _parsers;

        public AggregatedParser(IReadOnlyList<IParser<T>> parsers)
        {
            _parsers = parsers;
        }

        public AggregatedParser(params IParser<T>[] parsers)
            : this((IReadOnlyList<IParser<T>>)parsers)
        {
        }

        public IReadOnlyList<ParseResult<T>> Parse(string input, int startIndex, int length)
        {
            var results = new List<ParseResult<T>>();

            // Loop through each parser and process parts of input that weren't matched yet
            foreach (var parser in _parsers)
            {
                var offset = startIndex;

                // Process ranges between 0 and first result, first result and second result, etc
                for (var i = 0; i < results.Count; i++)
                {
                    var result = results[i];

                    // Parse inside this segment
                    var segmentResults = parser.Parse(input, offset, result.StartIndex - offset);

                    // Insert before this result
                    results.InsertRange(i, segmentResults);

                    // Shift index by number of inserted results
                    i += segmentResults.Count;

                    // Advance offset
                    offset = result.StartIndex + result.Length;
                }

                // Process range between last result and EOL
                if (offset < input.Length)
                {
                    // Parse inside this segment
                    var segmentResults = parser.Parse(input, offset, input.Length - offset);

                    // Append to the end of the list
                    results.AddRange(segmentResults);
                }
            }

            return results;
        }
    }
}