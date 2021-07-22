using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data.Embeds
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-field-structure
    public partial class EmbedField
    {
        public string Name { get; }

        public string Value { get; }

        public bool IsInline { get; }

        public EmbedField(string name, string value, bool isInline)
        {
            Name = name;
            Value = value;
            IsInline = isInline;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{Name} | {Value}";
    }

    public partial class EmbedField
    {
        public static EmbedField Parse(JsonElement json)
        {
            var name = json.GetProperty("name").GetString();
            var value = json.GetProperty("value").GetString();
            var isInline = json.GetPropertyOrNull("inline")?.GetBoolean() ?? false;

            return new EmbedField(name, value, isInline);
        }
    }
}