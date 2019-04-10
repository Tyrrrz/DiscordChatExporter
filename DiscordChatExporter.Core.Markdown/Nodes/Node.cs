namespace DiscordChatExporter.Core.Markdown.Nodes
{
    public abstract class Node
    {
        public string Source { get; }

        protected Node(string source)
        {
            Source = source;
        }
    }
}