using System.Diagnostics.CodeAnalysis;

namespace DiscordChatExporter.Core.Markdown
{
    internal class MultiLineCodeBlockNode : MarkdownNode
    {
        public string Language { get; }

        public string Code { get; }

        public MultiLineCodeBlockNode(string language, string code)
        {
            Language = language;
            Code = code;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"<{Language}> {Code}";
    }
}