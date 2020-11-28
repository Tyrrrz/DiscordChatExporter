using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async ValueTask DownloadAsync(this HttpClient httpClient, string uri, string outputFilePath)
        {
            await using var input = await httpClient.GetStreamAsync(uri);
            var output = File.Create(outputFilePath);

            await input.CopyToAsync(output);
            await output.DisposeAsync();
        }
    }
}