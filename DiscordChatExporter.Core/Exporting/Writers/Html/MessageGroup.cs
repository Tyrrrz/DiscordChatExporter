﻿using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Writers.Html
{
    // Used for grouping contiguous messages in HTML export
    internal partial class MessageGroup
    {
        public User Author { get; }

        public DateTimeOffset Timestamp { get; }

        public IReadOnlyList<Message> Messages { get; }

        public MessageReference? Reference { get; }

        public Message? ReferencedMessage {get; }

        public MessageGroup(
            User author,
            DateTimeOffset timestamp,
            MessageReference? reference,
            Message? referencedMessage,
            IReadOnlyList<Message> messages)
        {
            Author = author;
            Timestamp = timestamp;
            Reference = reference;
            ReferencedMessage = referencedMessage;
            Messages = messages;
        }
    }

    internal partial class MessageGroup
    {
        public static bool CanJoin(Message message1, Message message2) =>
            message1.Author.Id == message2.Author.Id &&
            string.Equals(message1.Author.FullName, message2.Author.FullName, StringComparison.Ordinal) &&
            (message2.Timestamp - message1.Timestamp).Duration().TotalMinutes <= 7 &&
            message2.Reference is null;

        public static MessageGroup Join(IReadOnlyList<Message> messages)
        {
            var first = messages.First();

            return new MessageGroup(
                first.Author,
                first.Timestamp,
                first.Reference,
                first.ReferencedMessage,
                messages
            );
        }
    }
}