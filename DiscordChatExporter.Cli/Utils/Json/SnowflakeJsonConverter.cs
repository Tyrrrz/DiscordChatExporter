using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Cli.Utils.Json;

internal class SnowflakeJsonConverter : JsonConverter<Snowflake>
{
    public override Snowflake Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => Snowflake.Parse(reader.GetString()!);

    public override void Write(
        Utf8JsonWriter writer,
        Snowflake value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.ToString());
}
