using System;
using System.IO;
using System.Linq;
using DiscordChatExporter.Domain.Discord.Models.Common;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/channel#attachment-object

    public partial class Attachment : IHasId
    {
        public string Id { get; }

        public string Url { get; }

        public string FileName { get; }

        public int? Width { get; }

        public int? Height { get; }

        public bool IsImage => ImageFileExtensions.Contains(Path.GetExtension(FileName), StringComparer.OrdinalIgnoreCase);

        public bool IsSpoiler => IsImage && FileName.StartsWith("SPOILER_", StringComparison.Ordinal);

        public FileSize FileSize { get; }

        public Attachment(string id, string url, string fileName, int? width, int? height, FileSize fileSize)
        {
            Id = id;
            Url = url;
            FileName = fileName;
            Width = width;
            Height = height;
            FileSize = fileSize;
        }

        public override string ToString() => FileName;
    }

    public partial class Attachment
    {
        private static readonly string[] ImageFileExtensions = {".jpg", ".jpeg", ".png", ".gif", ".bmp"};
    }
}