using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async ValueTask DownloadAsync(this HttpClient httpClient, string uri, string outputFilePath)
        {
            using var response = await httpClient.GetAsync(uri);
            var output = File.Create(outputFilePath);

            await response.Content.CopyToAsync(output);

            IEnumerable<string> lastModifiedHeaderValues;
            if (response.Content.Headers.TryGetValues("Last-Modified", out lastModifiedHeaderValues))
            {
                await output.DisposeAsync();
                File.SetLastWriteTime(outputFilePath, DateTime.Parse(lastModifiedHeaderValues.First()));
            }
        }
    }
}