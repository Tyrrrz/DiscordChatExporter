using System;
using System.Net.Http;

namespace DiscordChatExporter.Core.Exceptions;

public partial class DiscordChatExporterException : Exception
{
    public bool IsFatal { get; }

    public DiscordChatExporterException(string message, bool isFatal = false)
        : base(message)
    {
        IsFatal = isFatal;
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
        new("Authentication token is invalid.", true);

    internal static DiscordChatExporterException Forbidden() =>
        new("Access is forbidden.");

    internal static DiscordChatExporterException NotFound(string resourceId) =>
        new($"Requested resource ({resourceId}) does not exist.");

    internal static DiscordChatExporterException ChannelIsEmpty() =>
        new("No messages found for the specified period.");
}