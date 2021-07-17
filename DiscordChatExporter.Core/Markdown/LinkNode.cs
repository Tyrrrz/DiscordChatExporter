namespace DiscordChatExporter.Core.Markdown
{
    internal class LinkNode : MarkdownNode
    {
        public string Url { get; }

        public string Title { get; }

        public LinkNode(string url, string title)
        {
            Url = url;
            Title = title;
        }

        public LinkNode(string url)
            : this(url, url)
        {
        }

        public override string ToString() => $"<Link> {Title}";
    }
}