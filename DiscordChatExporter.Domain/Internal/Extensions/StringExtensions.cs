using System.Text;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static StringBuilder AppendLineIfNotNullOrWhiteSpace(this StringBuilder builder, string? value) =>
            !string.IsNullOrWhiteSpace(value) ? builder.AppendLine(value) : builder;

        public static StringBuilder Trim(this StringBuilder builder)
        {
            while (builder.Length > 0 && char.IsWhiteSpace(builder[0]))
                builder.Remove(0, 1);

            while (builder.Length > 0 && char.IsWhiteSpace(builder[^1]))
                builder.Remove(builder.Length - 1, 1);

            return builder;
        }
    }
}