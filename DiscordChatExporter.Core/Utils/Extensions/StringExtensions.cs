using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class StringExtensions
{
    public static string? NullIfWhiteSpace(this string str) =>
        !string.IsNullOrWhiteSpace(str)
            ? str
            : null;

    public static string Truncate(this string str, int charCount) =>
        str.Length > charCount
            ? str[..charCount]
            : str;

    public static IEnumerable<Rune> GetRunes(this string str)
    {
        var lastIndex = 0;
        while (lastIndex < str.Length && Rune.TryGetRuneAt(str, lastIndex, out var rune))
        {
            yield return rune;
            lastIndex += rune.Utf16SequenceLength;
        }
    }

    public static string ToDashCase(this string str) =>
        Regex.Replace(str, @"(\p{Ll})(\p{Lu})", "$1-$2");

    public static T? ParseEnumOrNull<T>(this string str, bool ignoreCase = true) where T : struct, Enum =>
        Enum.TryParse<T>(str, ignoreCase, out var result)
            ? result
            : null;

    public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
        builder.Length > 0
            ? builder.Append(value)
            : builder;
}