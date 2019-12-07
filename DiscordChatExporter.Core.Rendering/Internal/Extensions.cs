using System.Text;

namespace DiscordChatExporter.Core.Rendering.Internal
{
    internal static class Extensions
    {
        public static StringBuilder AppendLineIfNotEmpty(this StringBuilder builder, string value) =>
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