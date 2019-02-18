namespace DiscordChatExporter.Core.Markdown
{
    public class MultilineCodeBlockNode : Node
    {
        public string Language { get; }

        public string Text { get; }

        public MultilineCodeBlockNode(string language, string text)
        {
            Language = language;
            Text = text;
        }

        public override string ToString() => $"<Code {Language}> {Text}";
    }
}