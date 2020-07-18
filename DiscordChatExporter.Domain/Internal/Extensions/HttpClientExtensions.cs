using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class HttpClientExtensions
    {
        // HACK: ConfigureAwait() is crucial here to enable sync-over-async in HtmlMessageWriter
        public static async ValueTask DownloadAsync(this HttpClient httpClient, string uri, string outputFilePath)
        {
            await using var input = await httpClient.GetStreamAsync(uri).ConfigureAwait(false);
            var output = File.Create(outputFilePath);

            await input.CopyToAsync(output).ConfigureAwait(false);
            await output.DisposeAsync().ConfigureAwait(false);
        }

        public static async ValueTask<JsonElement> ReadAsJsonAsync(this HttpContent content)
        {
            await using var stream = await content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            return doc.RootElement.Clone();
        }
    }
}