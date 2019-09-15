namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class TextNode : Node
    {
        public string Text { get; }

        public TextNode(string text)
        {
            Text = text;
        }

        public override string ToString() => Text;
    }
}