using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    internal class ContainsMessageFilter : MessageFilter
    {
        private readonly string _text;

        public ContainsMessageFilter(string text) => _text = text;

        private bool Filter(string? content) =>
            !string.IsNullOrWhiteSpace(content) &&
            Regex.IsMatch(
                content,
                "\\b" + Regex.Escape(_text) + "\\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
            );

        public override bool Filter(Message message) =>
            Filter(message.Content) ||
            message.Embeds.Any(e =>
                Filter(e.Title) ||
                Filter(e.Author?.Name) ||
                Filter(e.Description) ||
                Filter(e.Footer?.Text) ||
                e.Fields.Any(f =>
                    Filter(f.Name) ||
                    Filter(f.Value)
                )
            );
    }
}