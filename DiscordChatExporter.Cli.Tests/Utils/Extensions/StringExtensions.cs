using System.Text;

namespace DiscordChatExporter.Cli.Tests.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string ReplaceWhiteSpace(string replacement = " ")
        {
            var buffer = new StringBuilder(str.Length);

            foreach (var ch in str)
                buffer.Append(char.IsWhiteSpace(ch) ? replacement : ch);

            return buffer.ToString();
        }
    }
}
