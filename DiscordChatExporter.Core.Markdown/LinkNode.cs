namespace DiscordChatExporter.Core.Markdown
{
    public class LinkNode : Node
    {
        public string Url { get; }

        public string Title { get; }

        public LinkNode(string lexeme, string url, string title)
            : base(lexeme)
        {
            Url = url;
            Title = title;
        }

        public LinkNode(string lexeme, string url) : this(lexeme, url, url)
        {
        }

        public override string ToString() => $"<Link> {Title}";
    }
}