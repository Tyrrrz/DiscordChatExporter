using System;
using System.Net;
using System.Net.Http;

namespace DiscordChatExporter.Domain.Internal
{
    internal static class Singleton
    {
        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler();

            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            handler.UseCookies = false;

            return new HttpClient(handler, true);
        });

        public static HttpClient HttpClient { get; } = LazyHttpClient.Value;
    }
}