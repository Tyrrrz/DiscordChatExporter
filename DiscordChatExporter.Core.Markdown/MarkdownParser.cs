using System.Collections.Generic;
using DiscordChatExporter.Core.Markdown.Internal;

namespace DiscordChatExporter.Core.Markdown
{
    public class MarkdownParser
    {
        public IReadOnlyList<Node> Parse(string input) => MarkdownGrammar.BuildTree(input);
    }
}