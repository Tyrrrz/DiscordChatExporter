using System.Collections.Generic;
using DiscordChatExporter.Core.Markdown.Internal;

namespace DiscordChatExporter.Core.Markdown
{
    public static class MarkdownParser
    {
        public static IReadOnlyList<Node> Parse(string input) => Grammar.BuildTree(input);
    }
}