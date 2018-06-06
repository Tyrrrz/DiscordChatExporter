namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-field-structure

    public class EmbedField
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
    }
}