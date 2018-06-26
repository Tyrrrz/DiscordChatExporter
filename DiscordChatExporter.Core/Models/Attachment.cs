namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#attachment-object

    public class Attachment
    {
        public string Id { get; }

        public string Url { get; }

        public int? Width { get; }

        public int? Height { get; }

        public bool IsImage => Width != null;

        public string FileName { get; }

        public long FileSize { get; }

        public Attachment(string id, int? width, int? height, string url, string fileName, long fileSize)
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