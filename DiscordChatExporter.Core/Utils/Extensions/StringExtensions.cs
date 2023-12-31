using System;
using System.Text;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class StringExtensions
{
    public static string? NullIfWhiteSpace(this string str) =>
        !string.IsNullOrWhiteSpace(str) ? str : null;

    public static string Truncate(this string str, int charCount) =>
        str.Length > charCount ? str[..charCount] : str;

    public static string ToSpaceSeparatedWords(this string str)
    {
        var builder = new StringBuilder(str.Length * 2);

        foreach (var c in str)
        {
            if (char.IsUpper(c) && builder.Length > 0)
                builder.Append(' ');

            builder.Append(c);
        }

        return builder.ToString();
    }

    public static T? ParseEnumOrNull<T>(this string str, bool ignoreCase = true)
        where T : struct, Enum => Enum.TryParse<T>(str, ignoreCase, out var result) ? result : null;

    public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
        builder.Length > 0 ? builder.Append(value) : builder;
}
