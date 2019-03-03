using System;
using ByteSizeLib;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#attachment-object

    public class Attachment
    {
        public string Id { get; }

        public string Url { get; }

        public int? Width { get; }

        public int? Height { get; }

        public string FileName { get; }

        public bool IsImage => FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                               FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                               FileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                               FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase);

        public ByteSize FileSize { get; }

        public Attachment(string id, int? width, int? height, string url, string fileName, ByteSize fileSize)
        {
            Id = id;
            Url = url;
            Width = width;
            Height = height;
            FileName = fileName;
            FileSize = fileSize;
        }

        public override string ToString() => FileName;
    }
}