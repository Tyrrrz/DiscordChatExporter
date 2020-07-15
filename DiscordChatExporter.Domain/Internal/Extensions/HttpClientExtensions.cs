using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async Task DownloadAsync(this HttpClient httpClient, string uri, string outputFilePath)
        {
            await using var input = await httpClient.GetStreamAsync(uri);
            await using var output = File.Create(outputFilePath);

            await input.CopyToAsync(output);
        }

        public static async Task<JsonElement> ReadAsJsonAsync(this HttpContent content)
        {
            await using var stream = await content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            return doc.RootElement.Clone();
        }
    }
}