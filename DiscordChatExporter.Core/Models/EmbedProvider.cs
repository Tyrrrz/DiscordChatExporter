using System;
using System.Collections.Generic;

// https://discordapp.com/developers/docs/resources/channel#embed-object-embed-provider-structure

namespace DiscordChatExporter.Core.Models
{
    public class EmbedProvider
    {
        public string Name { get; }

        public string Url { get; }

        public EmbedProvider(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}