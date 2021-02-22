namespace DiscordChatExporter.Core.Markdown.Ast
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

        public override string ToString() => $"<{Language}> {Code}";
    }
}