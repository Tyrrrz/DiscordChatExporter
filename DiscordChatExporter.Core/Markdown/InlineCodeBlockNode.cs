namespace DiscordChatExporter.Core.Markdown
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