namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/channel#attachment-object

    public class Attachment
    {
        public string Id { get; }

        public bool IsImage { get; }

        public string Url { get; }

        public string FileName { get; }

        public long FileSize { get; }

        public Attachment(string id, bool isImage, string url, string fileName, long fileSize)
        {
            Id = id;
            IsImage = isImage;
            Url = url;
            FileName = fileName;
            FileSize = fileSize;
        }

        public override string ToString() => FileName;
    }
}