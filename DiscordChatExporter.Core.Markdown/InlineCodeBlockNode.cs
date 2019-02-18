namespace DiscordChatExporter.Core.Markdown
{
    public class InlineCodeBlockNode : Node
    {
        public string Text { get; }

        public InlineCodeBlockNode(string text)
        {
            Text = text;
        }

        public override string ToString() => $"<Code> {Text}";
    }
}