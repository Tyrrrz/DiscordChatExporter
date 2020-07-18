using System.Text.Json;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out var result) && result.ValueKind != JsonValueKind.Null
                ? result
                : (JsonElement?) null;
    }
}