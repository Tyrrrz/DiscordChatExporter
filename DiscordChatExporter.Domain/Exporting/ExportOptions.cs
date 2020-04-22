namespace DiscordChatExporter.Domain.Exporting
{
    public class ExportOptions
    {
        public string BaseFilePath { get; }

        public ExportFormat Format { get; }

        public int? PartitionLimit { get; }

        public ExportOptions(string baseFilePath, ExportFormat format, int? partitionLimit)
        {
            BaseFilePath = baseFilePath;
            Format = format;
            PartitionLimit = partitionLimit;
        }
    }
}