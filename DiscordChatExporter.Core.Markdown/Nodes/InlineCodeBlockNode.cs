namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class InlineCodeBlockNode : Node
    {
        public string Code { get; }

        public InlineCodeBlockNode(string code)
        {
            Code = code;
        }

        public override string ToString() => $"<Code> {Code}";
    }
}