namespace DiscordChatExporter.Models
{
    public class Attachment
    {
        public string Id { get; }

        public string Url { get; }

        public string FileName { get; }

        public bool IsImage { get; }

        public Attachment(string id, string url, string fileName, bool isImage)
        {
            Id = id;
            Url = url;
            FileName = fileName;
            IsImage = isImage;
        }
    }
}