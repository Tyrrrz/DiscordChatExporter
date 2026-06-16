using System;

namespace DiscordChatExporter.Core.Exceptions;

public class DiscordChatExporterException(
    string message,
    bool isFatal = false,
    Exception? innerException = null
) : Exception(message, innerException)
{
    public bool IsFatal { get; } = isFatal;
}
