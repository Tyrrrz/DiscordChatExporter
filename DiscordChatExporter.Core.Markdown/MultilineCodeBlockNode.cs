namespace DiscordChatExporter.Core.Markdown
{
    public class MultilineCodeBlockNode : Node
    {
        public string Language { get; }

        public string Code { get; }

        public MultilineCodeBlockNode(string lexeme, string language, string code)
            : base(lexeme)
        {
            Language = language;
            Code = code;
        }

        public override string ToString() => $"<Code [{Language}]> {Code}";
    }
}