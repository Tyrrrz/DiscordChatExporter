using System.Diagnostics.CodeAnalysis;

namespace DiscordChatExporter.Core.Markdown
{
    internal class MentionNode : MarkdownNode
    {
        public string Id { get; }

        public MentionKind Kind { get; }

        public MentionNode(string id, MentionKind kind)
        {
            Id = id;
            Kind = kind;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"<{Kind} mention> {Id}";
    }
}