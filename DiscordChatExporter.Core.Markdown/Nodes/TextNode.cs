namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class TextNode : Node
    {
        public string Text { get; }

        public TextNode(string source, string text)
            : base(source)
        {
            Text = text;
        }

        public TextNode(string text) : this(text, text)
        {
        }

        public override string ToString() => Text;
    }
}