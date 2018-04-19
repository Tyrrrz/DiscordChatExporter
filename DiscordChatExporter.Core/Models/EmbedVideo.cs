using System;
using System.Collections.Generic;

// https://discordapp.com/developers/docs/resources/channel#embed-object-embed-video-structure

namespace DiscordChatExporter.Core.Models
{
    public class EmbedVideo
    {
        public string Url { get; }

        public int? Height { get; }

        public int? Width { get; }

        public EmbedVideo(string url, int? height, int? width)
        {
            Url = url;
            Height = height;
            Width = width;
        }
    }
}