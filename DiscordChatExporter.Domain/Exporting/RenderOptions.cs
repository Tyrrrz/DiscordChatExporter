namespace DiscordChatExporter.Domain.Exporting
{
    public class RenderOptions
    {
        public string BaseFilePath { get; }

        public ExportFormat Format { get; }

        public int? PartitionLimit { get; }

        public RenderOptions(string baseFilePath, ExportFormat format, int? partitionLimit)
        {
            BaseFilePath = baseFilePath;
            Format = format;
            PartitionLimit = partitionLimit;
        }
    }
}