using System;
using System.Net;

namespace DiscordChatExporter.Core.Services.Exceptions
{
    public class HttpErrorStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public string ReasonPhrase { get; }

        public HttpErrorStatusCodeException(HttpStatusCode statusCode, string reasonPhrase)
            : base($"Error HTTP status code: {statusCode} - {reasonPhrase}")
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }
    }
}