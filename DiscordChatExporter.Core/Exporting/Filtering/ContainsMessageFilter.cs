using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class ContainsMessageFilter : MessageFilter
{
    private readonly string _text;

    public ContainsMessageFilter(string text) => _text = text;

    private bool IsMatch(string? content) =>
        !string.IsNullOrWhiteSpace(content) &&
        Regex.IsMatch(
            content,
            "\\b" + Regex.Escape(_text) + "\\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

    public override bool IsMatch(Message message) =>
        IsMatch(message.Content) ||
        message.Embeds.Any(e =>
            IsMatch(e.Title) ||
            IsMatch(e.Author?.Name) ||
            IsMatch(e.Description) ||
            IsMatch(e.Footer?.Text) ||
            e.Fields.Any(f =>
                IsMatch(f.Name) ||
                IsMatch(f.Value)
            )
        );
}