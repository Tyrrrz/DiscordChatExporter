using System;
using System.Text;
using System.Text.Json;

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

        public static void WriteString(this Utf8JsonWriter writer, string propertyName, DateTimeOffset? value)
        {
            writer.WritePropertyName(propertyName);

            if (value != null)
                writer.WriteStringValue(value.Value);
            else
                writer.WriteNullValue();
        }

        public static void WriteNumber(this Utf8JsonWriter writer, string propertyName, int? value)
        {
            writer.WritePropertyName(propertyName);

            if (value != null)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}