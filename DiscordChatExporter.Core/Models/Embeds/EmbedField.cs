namespace DiscordChatExporter.Core.Models.Embeds
{
    // https://discordapp.com/developers/docs/resources/channel#embed-object-embed-field-structure

    public class EmbedField
    {
        public string Name { get; }

        public string Value { get; }

        public bool? Inline { get; }

        public EmbedField(string name, string value, bool? inline)
        {
            Name = name;
            Value = value;
            Inline = inline;
        }
    }
}