namespace DiscordChatExporter.Core.Markdown
{
    public class InlineCodeBlockNode : Node
    {
        public string Code { get; }

        public InlineCodeBlockNode(string lexeme, string code)
            : base(lexeme)
        {
            Code = code;
        }

        public override string ToString() => $"<Code> {Code}";
    }
}