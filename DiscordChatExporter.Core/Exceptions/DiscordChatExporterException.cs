﻿using System;
using System.Net.Http;

namespace DiscordChatExporter.Core.Exceptions
{
    public partial class DiscordChatExporterException : Exception
    {
        public bool IsCritical { get; }

        public DiscordChatExporterException(string message, bool isCritical = false)
            : base(message)
        {
            IsCritical = isCritical;
        }
    }

    public partial class DiscordChatExporterException
    {
        internal static DiscordChatExporterException FailedHttpRequest(HttpResponseMessage response)
        {
            var message = $@"
Failed to perform an HTTP request.

[Request]
{response.RequestMessage}

[Response]
{response}";

            return new DiscordChatExporterException(message.Trim(), true);
        }

        internal static DiscordChatExporterException Unauthorized() =>
            new("Authentication token is invalid.");

        internal static DiscordChatExporterException Forbidden() =>
            new("Access is forbidden.");

        internal static DiscordChatExporterException NotFound() =>
            new("Requested resource does not exist.");

        internal static DiscordChatExporterException ChannelIsEmpty() =>
            new("No messages found for the specified period.");
    }
}