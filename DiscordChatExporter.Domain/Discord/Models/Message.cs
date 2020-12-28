using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Utilities;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discord.com/developers/docs/resources/channel#message-object-message-types
    public enum MessageType
    {
        Default,
        RecipientAdd,
        RecipientRemove,
        Call,
        ChannelNameChange,
        ChannelIconChange,
        ChannelPinnedMessage,
        GuildMemberJoin,
        Reply = 19
    }

    // https://discord.com/developers/docs/resources/channel#message-object
    public partial class Message : IHasId
    {
        public Snowflake Id { get; }

        public MessageType Type { get; }

        public User Author { get; }

        public DateTimeOffset Timestamp { get; }

        public DateTimeOffset? EditedTimestamp { get; }

        public DateTimeOffset? CallEndedTimestamp { get; }

        public bool IsPinned { get; }

        public string Content { get; }

        public IReadOnlyList<Attachment> Attachments { get; }

        public IReadOnlyList<Embed> Embeds { get; }

        public IReadOnlyList<Reaction> Reactions { get; }

        public IReadOnlyList<User> MentionedUsers { get; }

        public MessageReference? Reference {get; }

        public Message? ReferencedMessage {get; }

        public Message(
            Snowflake id,
            MessageType type,
            User author,
            DateTimeOffset timestamp,
            DateTimeOffset? editedTimestamp,
            DateTimeOffset? callEndedTimestamp,
            bool isPinned,
            string content,
            IReadOnlyList<Attachment> attachments,
            IReadOnlyList<Embed> embeds,
            IReadOnlyList<Reaction> reactions,
            IReadOnlyList<User> mentionedUsers,
            MessageReference? messageReference,
            Message? referencedMessage)
        {
            Id = id;
            Type = type;
            Author = author;
            Timestamp = timestamp;
            EditedTimestamp = editedTimestamp;
            CallEndedTimestamp = callEndedTimestamp;
            IsPinned = isPinned;
            Content = content;
            Attachments = attachments;
            Embeds = embeds;
            Reactions = reactions;
            MentionedUsers = mentionedUsers;
            Reference = messageReference;
            ReferencedMessage = referencedMessage;
        }

        public override string ToString() => Content;
    }

    public partial class Message
    {
        public static Message Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var author = json.GetProperty("author").Pipe(User.Parse);
            var timestamp = json.GetProperty("timestamp").GetDateTimeOffset();
            var editedTimestamp = json.GetPropertyOrNull("edited_timestamp")?.GetDateTimeOffset();
            var callEndedTimestamp = json.GetPropertyOrNull("call")?.GetPropertyOrNull("ended_timestamp")?.GetDateTimeOffset();
            var type = (MessageType) json.GetProperty("type").GetInt32();
            var isPinned = json.GetPropertyOrNull("pinned")?.GetBoolean() ?? false;
            var messageReference = json.GetPropertyOrNull("message_reference")?.Pipe(MessageReference.Parse);
            var referencedMessage = json.GetPropertyOrNull("referenced_message")?.Pipe(Message.Parse);

            var content = type switch
            {
                MessageType.RecipientAdd => "Added a recipient.",
                MessageType.RecipientRemove => "Removed a recipient.",
                MessageType.Call =>
                $"Started a call that lasted {callEndedTimestamp?.Pipe(t => t - timestamp).Pipe(t => (int) t.TotalMinutes) ?? 0} minutes.",
                MessageType.ChannelNameChange => "Changed the channel name.",
                MessageType.ChannelIconChange => "Changed the channel icon.",
                MessageType.ChannelPinnedMessage => "Pinned a message.",
                MessageType.GuildMemberJoin => "Joined the server.",
                _ => json.GetPropertyOrNull("content")?.GetString() ?? ""
            };

            var attachments =
                json.GetPropertyOrNull("attachments")?.EnumerateArray().Select(Attachment.Parse).ToArray() ??
                Array.Empty<Attachment>();

            var embeds =
                json.GetPropertyOrNull("embeds")?.EnumerateArray().Select(Embed.Parse).ToArray() ??
                Array.Empty<Embed>();

            var reactions =
                json.GetPropertyOrNull("reactions")?.EnumerateArray().Select(Reaction.Parse).ToArray() ??
                Array.Empty<Reaction>();

            var mentionedUsers =
                json.GetPropertyOrNull("mentions")?.EnumerateArray().Select(User.Parse).ToArray() ??
                Array.Empty<User>();

            return new Message(
                id,
                type,
                author,
                timestamp,
                editedTimestamp,
                callEndedTimestamp,
                isPinned,
                content,
                attachments,
                embeds,
                reactions,
                mentionedUsers,
                messageReference,
                referencedMessage
            );
        }
    }
}