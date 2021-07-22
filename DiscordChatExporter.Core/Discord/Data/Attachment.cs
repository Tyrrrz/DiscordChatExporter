using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/channel#attachment-object
    public partial class Attachment : IHasId
    {
        public Snowflake Id { get; }

        public string Url { get; }

        public string FileName { get; }

        public string FileExtension => Path.GetExtension(FileName);

        public int? Width { get; }

        public int? Height { get; }

        public bool IsImage => FileFormat.IsImage(FileExtension);

        public bool IsVideo => FileFormat.IsVideo(FileExtension);

        public bool IsAudio => FileFormat.IsAudio(FileExtension);

        public bool IsSpoiler => FileName.StartsWith("SPOILER_", StringComparison.Ordinal);

        public FileSize FileSize { get; }

        public Attachment(
            Snowflake id,
            string url,
            string fileName,
            int? width,
            int? height,
            FileSize fileSize)
        {
            Id = id;
            Url = url;
            FileName = fileName;
            Width = width;
            Height = height;
            FileSize = fileSize;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => FileName;
    }

    public partial class Attachment
    {
        public static Attachment Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var url = json.GetProperty("url").GetString();
            var width = json.GetPropertyOrNull("width")?.GetInt32();
            var height = json.GetPropertyOrNull("height")?.GetInt32();
            var fileName = json.GetProperty("filename").GetString();
            var fileSize = json.GetProperty("size").GetInt64().Pipe(FileSize.FromBytes);

            return new Attachment(id, url, fileName, width, height, fileSize);
        }
    }
}