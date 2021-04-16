namespace DiscordChatExporter.Core.Exporting.Writers.Html
{
    internal class PreambleTemplateContext
    {
        public ExportContext ExportContext { get; }

        public string ThemeName { get; }

        public PreambleTemplateContext(ExportContext exportContext, string themeName)
        {
            ExportContext = exportContext;
            ThemeName = themeName;
        }
    }
}