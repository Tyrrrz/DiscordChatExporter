namespace DiscordChatExporter.Core.Markdown
{
    public class MultilineCodeBlockNode : Node
    {
        public string Language { get; }

        public string Code { get; }

        public MultilineCodeBlockNode(string language, string code)
        {
            Language = language;
            Code = code;
        }

        public MultilineCodeBlockNode(string code)
            : this(null, code)
        {
        }

        public override string ToString() => $"<Code {Language}> {Code}";
    }
}