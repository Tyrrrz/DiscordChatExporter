using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/channel#message-object-message-types
    public enum MessageKind
    {
        Default = 0,
        RecipientAdd = 1,
        RecipientRemove = 2,
        Call = 3,
        ChannelNameChange = 4,
        ChannelIconChange = 5,
        ChannelPinnedMessage = 6,
        GuildMemberJoin = 7,
        Reply = 19
    }

    // https://discord.com/developers/docs/resources/channel#message-object
    public partial class Message : IHasId
    {
        public Snowflake Id { get; }

        public MessageKind Kind { get; }

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

        public MessageReference? Reference { get; }

        public Message? ReferencedMessage { get; }

        public Message(
            Snowflake id,
            MessageKind kind,
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
            Kind = kind;
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
            var kind = (MessageKind) json.GetProperty("type").GetInt32();
            var isPinned = json.GetPropertyOrNull("pinned")?.GetBoolean() ?? false;
            var messageReference = json.GetPropertyOrNull("message_reference")?.Pipe(MessageReference.Parse);
            var referencedMessage = json.GetPropertyOrNull("referenced_message")?.Pipe(Parse);

            var content = kind switch
            {
                MessageKind.RecipientAdd => "Added a recipient.",
                MessageKind.RecipientRemove => "Removed a recipient.",
                MessageKind.Call =>
                    $"Started a call that lasted {callEndedTimestamp?.Pipe(t => t - timestamp).Pipe(t => (int) t.TotalMinutes) ?? 0} minutes.",
                MessageKind.ChannelNameChange => "Changed the channel name.",
                MessageKind.ChannelIconChange => "Changed the channel icon.",
                MessageKind.ChannelPinnedMessage => "Pinned a message.",
                MessageKind.GuildMemberJoin => "Joined the server.",
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
                kind,
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