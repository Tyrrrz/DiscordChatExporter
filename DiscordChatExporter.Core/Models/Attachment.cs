namespace DiscordChatExporter.Core.Models
{
    public class Attachment
    {
        public string Id { get; }

        public AttachmentType Type { get; }

        public string Url { get; }

        public string FileName { get; }

        public long FileSize { get; }

        public Attachment(string id, AttachmentType type, string url, string fileName, long fileSize)
        {
            Id = id;
            Type = type;
            Url = url;
            FileName = fileName;
            FileSize = fileSize;
        }
    }
}