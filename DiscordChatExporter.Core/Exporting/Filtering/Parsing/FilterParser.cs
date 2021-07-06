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

        public static TokenListParser<FilterToken, string> AnyString { get; } =
            Token.EqualTo(FilterToken.QuotedString).Apply(QuotedString)
            .Or(Token.EqualTo(FilterToken.UnquotedString).Apply(UnquotedString));

        public static TokenListParser<FilterToken, MessageFilter> AnyFilter { get; } =
            from minus in Token.EqualTo(FilterToken.Minus).Optional()
            from content in KeyValueFilter.Or(TextFilter).Or(GroupedFilter)
            select minus.HasValue ? new NegatedMessageFilter(content) : content;

        public static TokenListParser<FilterToken, MessageFilter> TextFilter { get; } =
           from value in AnyString
           select MessageFilter.CreateFilter(value);

        public static TokenListParser<FilterToken, MessageFilter> KeyValueFilter { get; } =
            from key in AnyString.Try()
            from colon in Token.EqualTo(FilterToken.Colon).Try()
            from value in AnyString
            select MessageFilter.CreateFilter(key, value);

        public static TokenListParser<FilterToken, MessageFilter> GroupedFilter { get; } =
            from open in Token.EqualTo(FilterToken.LParen)
            from content in BinaryExpression
            from close in Token.EqualTo(FilterToken.RParen)
            select content;

        public static TokenListParser<FilterToken, MessageFilter> OrBinaryExpression { get; } =
            from first in AnyFilter
            from vbar in Token.EqualTo(FilterToken.VBar)
            from rest in BinaryExpression
            select (MessageFilter)new BinaryExpressionMessageFilter(first, rest, BinaryExpressionKind.Or);

        public static TokenListParser<FilterToken, MessageFilter> AndBinaryExpression { get; } =
            from first in AnyFilter
            from rest in BinaryExpression
            select (MessageFilter)new BinaryExpressionMessageFilter(first, rest, BinaryExpressionKind.And);

        public static TokenListParser<FilterToken, MessageFilter> BinaryExpression { get; } = OrBinaryExpression.Try().Or(AndBinaryExpression.Try()).Or(AnyFilter);
        
        public static TokenListParser<FilterToken, MessageFilter> Instance { get; } = BinaryExpression.AtEnd();
    }
}
