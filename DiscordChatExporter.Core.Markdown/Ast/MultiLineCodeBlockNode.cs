namespace DiscordChatExporter.Core.Markdown.Ast
{
    public class MultiLineCodeBlockNode : Node
    {
        public string Language { get; }

        public string Code { get; }

        public MultiLineCodeBlockNode(string language, string code)
        {
            Language = language;
            Code = code;
        }

        public override string ToString() => $"<Code [{Language}]> {Code}";
    }
}