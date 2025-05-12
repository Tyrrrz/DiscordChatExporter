namespace DiscordChatExporter.Core.Exceptions;

public class ChannelEmptyException(string message) : DiscordChatExporterException(message);
