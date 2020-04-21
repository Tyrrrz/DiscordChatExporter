using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordChatExporter.Domain.Internal
{
    internal static class HttpClientExtensions
    {
        public static async Task<JsonElement> ReadAsJsonAsync(this HttpContent content)
        {
            await using var stream = await content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            return doc.RootElement.Clone();
        }
    }
}