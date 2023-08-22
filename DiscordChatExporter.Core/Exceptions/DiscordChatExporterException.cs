using System;

namespace DiscordChatExporter.Core.Exceptions;

public class DiscordChatExporterException : Exception
{
    public bool IsFatal { get; }

    public DiscordChatExporterException(string message, bool isFatal = false)
        : base(message)
    {
        IsFatal = isFatal;
    }
}
