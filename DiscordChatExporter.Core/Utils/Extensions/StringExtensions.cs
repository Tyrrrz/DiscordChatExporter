using System.Text;

namespace DiscordChatExporter.Core.Utils.Extensions
{
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

        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
            builder.Length > 0
                ? builder.Append(value)
                : builder;
    }
}