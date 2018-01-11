namespace DiscordChatExporter.Messages
{
    public class ShowExportDoneMessage
    {
        public string FilePath { get; }

        public ShowExportDoneMessage(string filePath)
        {
            FilePath = filePath;
        }
    }
}