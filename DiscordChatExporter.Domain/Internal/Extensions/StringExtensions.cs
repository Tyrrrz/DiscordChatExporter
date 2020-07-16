using System.Text;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static StringBuilder AppendIfEmpty(this StringBuilder builder, char value) =>
            builder.Length == 0
                ? builder.Append(value)
                : builder;
    }
}