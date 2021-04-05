namespace DiscordChatExporter.Core.Markdown.Ast
{
    internal class InlineCodeBlockNode : MarkdownNode
    {
        public string Code { get; }

        public InlineCodeBlockNode(string code)
        {
            Code = code;
        }

        public override string ToString() => $"<Code> {Code}";
    }
}