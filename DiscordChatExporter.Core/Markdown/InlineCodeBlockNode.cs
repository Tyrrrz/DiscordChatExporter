using System.Diagnostics.CodeAnalysis;

namespace DiscordChatExporter.Core.Markdown
{
    internal class InlineCodeBlockNode : MarkdownNode
    {
        public string Code { get; }

        public InlineCodeBlockNode(string code)
        {
            Code = code;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"<Code> {Code}";
    }
}