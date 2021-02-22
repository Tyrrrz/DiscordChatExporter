using System.Text;

namespace DiscordChatExporter.Core.Utils.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string str, int charCount) =>
            str.Length > charCount
                ? str.Substring(0, charCount)
                : str;

        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
            builder.Length > 0
                ? builder.Append(value)
                : builder;
    }
}