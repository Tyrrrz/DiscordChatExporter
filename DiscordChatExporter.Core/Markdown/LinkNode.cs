using System.Diagnostics.CodeAnalysis;

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

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"<Link> {Title}";
    }
}