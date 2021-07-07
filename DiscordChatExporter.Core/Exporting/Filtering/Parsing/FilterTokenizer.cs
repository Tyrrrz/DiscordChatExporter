using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting.Filtering.Parsing
{
    public static class FilterTokenizer
    {
        public static Tokenizer<FilterToken> Instance { get; } = new TokenizerBuilder<FilterToken>()
            .Ignore(Span.WhiteSpace)
            .Match(Character.EqualTo('('), FilterToken.LParen)
            .Match(Character.EqualTo(')'), FilterToken.RParen)
            .Match(Character.EqualTo(':'), FilterToken.Colon)
            .Match(Character.EqualTo('-'), FilterToken.Minus)
            .Match(Character.EqualTo('|'), FilterToken.VBar)
            .Match(FilterParser.QuotedString, FilterToken.QuotedString)
            .Match(FilterParser.UnquotedString, FilterToken.UnquotedString)
            .Build();
    }
}
