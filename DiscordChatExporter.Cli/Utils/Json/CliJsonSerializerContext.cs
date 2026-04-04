using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Cli.Utils.Json;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(Channel[]))]
[JsonSerializable(typeof(Guild[]))]
internal partial class CliJsonSerializerContext : JsonSerializerContext
{
    // Instance pre-configured with converters for Snowflake (serialised as a string)
    // and all enum types (serialised as their name).  Defined here so the Core types
    // are never touched.
    public static CliJsonSerializerContext Instance { get; } =
        new(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new SnowflakeJsonConverter(), new JsonStringEnumConverter() },
            }
        );
}
