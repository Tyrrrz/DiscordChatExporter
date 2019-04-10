namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public class LinkNode : Node
    {
        public string Url { get; }

        public string Title { get; }

        public LinkNode(string source, string url, string title)
            : base(source)
        {
            Url = url;
            Title = title;
        }

        public LinkNode(string source, string url) : this(source, url, url)
        {
        }

        public override string ToString() => $"<Link> {Title}";
    }
}