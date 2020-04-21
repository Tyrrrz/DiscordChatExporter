using System.Text.Json;

namespace DiscordChatExporter.Core.Services.Internal
{
    internal static class Json
    {
        public static JsonElement Parse(string json)
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
    }
}