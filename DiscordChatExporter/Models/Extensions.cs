namespace DiscordChatExporter.Models
{
    public static class Extensions
    {
        public static string GetFileExtension(this ExportFormat format)
        {
            if (format == ExportFormat.Text)
                return "txt";
            if (format == ExportFormat.Html)
                return "html";
            return null;
        }
    }
}