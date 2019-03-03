namespace DiscordChatExporter.Core.Markdown
{
    public abstract class Node
    {
        public string Lexeme { get; }

        protected Node(string lexeme)
        {
            Lexeme = lexeme;
        }
    }
}