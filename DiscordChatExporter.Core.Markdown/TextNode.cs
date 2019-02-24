namespace DiscordChatExporter.Core.Markdown
{
    public class TextNode : Node
    {
        public string Text { get; }

        public TextNode(string lexeme, string text)
            : base(lexeme)
        {
            Text = text;
        }

        public TextNode(string text) : this(text, text)
        {
        }

        public override string ToString() => Text;
    }
}