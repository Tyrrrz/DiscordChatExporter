﻿using System.Collections.Generic;

namespace DiscordChatExporter.Core.Markdown.Parsing;

internal class AggregateMatcher<TContext, TValue>(
    IReadOnlyList<IMatcher<TContext, TValue>> matchers
) : IMatcher<TContext, TValue>
{
    public AggregateMatcher(params IMatcher<TContext, TValue>[] matchers)
        : this((IReadOnlyList<IMatcher<TContext, TValue>>)matchers) { }

    public ParsedMatch<TValue>? TryMatch(TContext context, StringSegment segment)
    {
        ParsedMatch<TValue>? earliestMatch = null;

        // Try to match the input with each matcher and get the match with the lowest start index
        foreach (var matcher in matchers)
        {
            // Try to match
            var match = matcher.TryMatch(context, segment);

            // If there's no match - continue
            if (match is null)
                continue;

            // If this match is earlier than previous earliest - replace
            if (
                earliestMatch is null
                || match.Segment.StartIndex < earliestMatch.Segment.StartIndex
            )
                earliestMatch = match;

            // If the earliest match starts at the very beginning - break,
            // because it's impossible to find a match earlier than that
            if (earliestMatch.Segment.StartIndex == segment.StartIndex)
                break;
        }

        return earliestMatch;
    }
}
