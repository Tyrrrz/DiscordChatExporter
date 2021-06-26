using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DiscordChatExporter.Core.Exporting.Filtering.Parsing
{
    public static class FilterParser
    {
        public static TokenListParser<FilterToken, string> AnyString { get; } =
            Token.EqualTo(FilterToken.QuotedString).Apply(FilterTextParsers.QuotedString)
            .Or(Token.EqualTo(FilterToken.UnquotedString).Apply(FilterTextParsers.UnquotedString));

        public static TokenListParser<FilterToken, MessageFilter> AnyFilter { get; } =
            from minus in Token.EqualTo(FilterToken.Minus).Optional()
            from content in KeyValuePair.Or(TextFilter.Try()).Or(GroupedFilter)
            select minus.HasValue ? new NegatedMessageFilter(content) : content;

        public static TokenListParser<FilterToken, MessageFilter> KeyValuePair { get; } =
            from key in AnyString.Try()
            from colon in Token.EqualTo(FilterToken.Colon).Try()
            from value in AnyString
            select MessageFilter.CreateFilter(key, value);

        public static TokenListParser<FilterToken, MessageFilter> TextFilter { get; } =
            from value in AnyString
            select MessageFilter.CreateFilter("contains", value);

        public static TokenListParser<FilterToken, MessageFilter> MultipleFilter { get; } =
            from first in AnyFilter
            from rest in Token.EqualTo(FilterToken.VBar)
                .IgnoreThen(AnyFilter)
                .Select(value => (Value: value, Unioned: true)) //anonymous struct keeps track of whether a vertical bar was used
                .Or(AnyFilter.Select(value => (Value: value, Unioned: false)))
                .Many()
            select rest.Aggregate(first, (current, next) => new BooleanMessageFilter(current, next.Value, next.Unioned));

        public static TokenListParser<FilterToken, MessageFilter> GroupedFilter { get; } =
            from open in Token.EqualTo(FilterToken.LParen)
            from content in MultipleFilter
            from close in Token.EqualTo(FilterToken.RParen)
            select content;

        public static TokenListParser<FilterToken, MessageFilter> Instance { get; } = MultipleFilter.AtEnd();

        public static bool TryParse(string input, [MaybeNullWhen(false)] out MessageFilter value, [MaybeNullWhen(true)] out string error, out Position errorPosition)
        {
            Console.WriteLine("tokenizing...");
            var tokens = FilterTokenizer.Instance.TryTokenize(input);
            if (!tokens.HasValue)
            {
                value = null;
                error = tokens.ToString();
                errorPosition = tokens.ErrorPosition;
                Console.WriteLine(error);
                return false;
            }

            Console.WriteLine("parsing...");
            var parsed = Instance.TryParse(tokens.Value);
            if (!parsed.HasValue)
            {
                value = null;
                error = parsed.ToString();
                errorPosition = parsed.ErrorPosition;
                Console.WriteLine(error);
                return false;
            }

            value = parsed.Value;
            error = null;
            errorPosition = Position.Empty;
            return true;
        }
    }
}
