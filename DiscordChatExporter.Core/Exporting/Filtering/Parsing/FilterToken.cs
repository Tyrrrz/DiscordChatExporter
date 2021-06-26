using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordChatExporter.Core.Exporting.Filtering.Parsing
{
    public enum FilterToken
    {
        None,
        LParen,
        RParen,
        Colon,
        Minus,
        VBar,
        UnquotedString,
        QuotedString
    }
}
