using System;

namespace DiscordChatExporter.Core.Exceptions;

// Thrown when there is circumstancially no message to export with given parameters,
// though it should not be treated as a runtime error; simply warn instead
public class ChannelEmptyException(string message)
    : DiscordChatExporterException(message, false, null)
{ }
