using System;
using System.Collections.Generic;

// https://discordapp.com/developers/docs/resources/channel#embed-object-embed-field-structure

namespace DiscordChatExporter.Core.Models
{
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