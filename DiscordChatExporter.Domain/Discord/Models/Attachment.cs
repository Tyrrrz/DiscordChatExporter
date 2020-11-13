using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discord.com/developers/docs/resources/channel#attachment-object
    public partial class Attachment : IHasId
    {
        public string Id { get; }

        public string Url { get; }

        public string FileName { get; }

        public int? Width { get; }

        public int? Height { get; }

        public bool IsImage =>
            ImageFileExtensions.Contains(Path.GetExtension(FileName), StringComparer.OrdinalIgnoreCase);

        public bool IsVideo =>
            WebSafeVideoFileExtensions.Contains(Path.GetExtension(FileName), StringComparer.OrdinalIgnoreCase);
        public bool IsAudio =>
            WebSafeAudioFileExtensions.Contains(Path.GetExtension(FileName), StringComparer.OrdinalIgnoreCase);

        public bool IsSpoiler =>
            IsImage && FileName.StartsWith("SPOILER_", StringComparison.Ordinal);

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
        private static readonly string[] ImageFileExtensions = {".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"};
        private static readonly string[] WebSafeVideoFileExtensions = { ".mp4", ".webm" };
        private static readonly string[] WebSafeAudioFileExtensions = { ".mp3", ".wav", ".ogg", ".flac", ".m4a" };

        public static Attachment Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString();
            var url = json.GetProperty("url").GetString();
            var width = json.GetPropertyOrNull("width")?.GetInt32();
            var height = json.GetPropertyOrNull("height")?.GetInt32();
            var fileName = json.GetProperty("filename").GetString();
            var fileSize = json.GetProperty("size").GetInt64().Pipe(FileSize.FromBytes);

            return new Attachment(id, url, fileName, width, height, fileSize);
        }
    }
}