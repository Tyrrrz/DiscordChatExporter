using System;
using System.IO;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#attachment-object

    public partial class Attachment : IHasId
    {
        public string Id { get; }

        public string Url { get; }

        public int? Width { get; }

        public int? Height { get; }

        public string FileName { get; }

        public bool IsImage { get; }

        public bool IsSpoiler { get; }

        public FileSize FileSize { get; }

        public Attachment(string id, int? width, int? height, string url, string fileName, FileSize fileSize)
        {
            Id = id;
            Url = url;
            Width = width;
            Height = height;
            FileName = fileName;
            FileSize = fileSize;

            IsImage = GetIsImage(fileName);

            IsSpoiler = IsImage && FileName.StartsWith("SPOILER_", StringComparison.Ordinal);
        }

        public override string ToString() => FileName;
    }

    public partial class Attachment
    {
        private static readonly string[] ImageFileExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public static bool GetIsImage(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            return ImageFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }
    }
}