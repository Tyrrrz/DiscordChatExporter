using System;
using System.Text.Json;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class Utf8JsonWriterExtensions
    {
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