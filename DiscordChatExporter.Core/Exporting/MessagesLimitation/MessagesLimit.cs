using System.Globalization;
using System;

namespace DiscordChatExporter.Core.Exporting.MessagesLimitation;

public abstract partial class MessagesLimit
{
    public abstract bool IsReached(int messagesCount);
}

public partial class MessagesLimit
{
    public static MessagesLimit Null { get; } = new NullMessagesLimit();

    public static MessagesLimit? TryParse(string value, IFormatProvider? formatProvider = null)
    {
        if (int.TryParse(value, NumberStyles.Integer, formatProvider, out var messageCountLimit))
            return new MessagesCountLimit(messageCountLimit);

        return null;
    }
    public static MessagesLimit Parse(string value, IFormatProvider? formatProvider = null) =>
       TryParse(value, formatProvider) ?? throw new FormatException($"Invalid messages limit '{value}'.");
}