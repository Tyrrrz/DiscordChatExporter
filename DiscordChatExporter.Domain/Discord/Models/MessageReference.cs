using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // reference data sent with crossposted messages and replies
    // https://discord.com/developers/docs/resources/channel#message-object-message-reference-structure
    public partial class MessageReference
    {
        public string? MessageId { get; }

        public string? ChannelId { get; }

        public string? GuildId { get; }

        public MessageReference(string? message_id, string? channel_id, string? guild_id)
        {
            MessageId = message_id;
            ChannelId = channel_id;
            GuildId = guild_id;
        }

        public override string ToString() => MessageId ?? "?";
    }

    public partial class MessageReference
    {
        public static MessageReference Parse(JsonElement json)
        {
            var message_id = json.GetPropertyOrNull("message_id")?.GetString();
            var channel_id = json.GetPropertyOrNull("channel_id")?.GetString();
            var guild_id = json.GetPropertyOrNull("guild_id")?.GetString();

            return new MessageReference(message_id, channel_id, guild_id);
        }
    }
}