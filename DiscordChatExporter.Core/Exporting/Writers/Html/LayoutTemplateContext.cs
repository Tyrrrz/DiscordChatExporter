namespace DiscordChatExporter.Core.Exporting.Writers.Html
{
    internal class LayoutTemplateContext
    {
        public ExportContext ExportContext { get; }

        public string ThemeName { get; }

        public long MessageCount { get; }

        public LayoutTemplateContext(ExportContext exportContext, string themeName, long messageCount)
        {
            ExportContext = exportContext;
            ThemeName = themeName;
            MessageCount = messageCount;
        }
    }
}