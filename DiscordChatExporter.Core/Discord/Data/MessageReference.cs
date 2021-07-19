using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/channel#message-object-message-reference-structure
    public partial class MessageReference
    {
        public Snowflake? MessageId { get; }

        public Snowflake? ChannelId { get; }

        public Snowflake? GuildId { get; }

        public MessageReference(Snowflake? messageId, Snowflake? channelId, Snowflake? guildId)
        {
            MessageId = messageId;
            ChannelId = channelId;
            GuildId = guildId;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => MessageId?.ToString() ?? "<unknown reference>";
    }

    public partial class MessageReference
    {
        public static MessageReference Parse(JsonElement json)
        {
            var messageId = json.GetPropertyOrNull("message_id")?.GetString().Pipe(Snowflake.Parse);
            var channelId = json.GetPropertyOrNull("channel_id")?.GetString().Pipe(Snowflake.Parse);
            var guildId = json.GetPropertyOrNull("guild_id")?.GetString().Pipe(Snowflake.Parse);

            return new MessageReference(messageId, channelId, guildId);
        }
    }
}