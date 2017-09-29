using System;
using System.Net;

namespace DiscordChatExporter.Exceptions
{
    public class HttpErrorStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpErrorStatusCodeException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}