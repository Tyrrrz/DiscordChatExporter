using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    public record ChannelCategory(Snowflake Id, string Name, int? Position) : IHasId
    {
        public static ChannelCategory Unknown { get; } = new(Snowflake.Zero, "<unknown category>", 0);

        public static ChannelCategory Parse(JsonElement json, int? position = null)
        {
            var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);

            var name =
                json.GetPropertyOrNull("name")?.GetStringOrNull() ??
                id.ToString();

            return new ChannelCategory(
                id,
                name,
                position ?? json.GetPropertyOrNull("position")?.GetInt32()
            );
        }
    }
}