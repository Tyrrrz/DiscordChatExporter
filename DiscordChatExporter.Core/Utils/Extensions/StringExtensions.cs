using System.Text;

namespace DiscordChatExporter.Core.Utils.Extensions;

public static class StringExtensions
{
    extension(string str)
    {
        public string? NullIfWhiteSpace() => !string.IsNullOrWhiteSpace(str) ? str : null;

        public string Truncate(int charCount) => str.Length > charCount ? str[..charCount] : str;

        public string ToSpaceSeparatedWords()
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
    }

    extension(StringBuilder builder)
    {
        public StringBuilder AppendIfNotEmpty(char value) =>
            builder.Length > 0 ? builder.Append(value) : builder;
    }
}
