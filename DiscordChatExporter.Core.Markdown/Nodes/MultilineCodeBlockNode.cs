namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class MultilineCodeBlockNode : Node
    {
        public string Language { get; }

        public string Code { get; }

        public MultilineCodeBlockNode(string source, string language, string code)
            : base(source)
        {
            Language = language;
            Code = code;
        }

        public override string ToString() => $"<Code [{Language}]> {Code}";
    }
}