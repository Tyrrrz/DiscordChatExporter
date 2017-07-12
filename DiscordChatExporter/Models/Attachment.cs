namespace DiscordChatExporter.Models
{
    public class Attachment
    {
        public string Id { get; }

        public string Url { get; }

        public string FileName { get; }

        public long ContentLength { get; }

        public Attachment(string id, string url, string fileName, long contentLength)
        {
            Id = id;
            Url = url;
            FileName = fileName;
            ContentLength = contentLength;
        }
    }
}