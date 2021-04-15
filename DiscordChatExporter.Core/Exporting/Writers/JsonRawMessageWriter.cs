using System.IO;

namespace DiscordChatExporter.Core.Exporting.Writers
{
    internal class JsonRawMessageWriter : JsonMessageWriter
    {
        public JsonRawMessageWriter(Stream stream, ExportContext context) : base(stream, context) {}

        protected override string FormatMarkdown(string? markdown) => markdown ?? "";

    }
}