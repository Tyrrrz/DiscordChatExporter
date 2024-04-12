using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering;

internal class ContainsMessageFilter(string text) : MessageFilter
{
    // Match content within word boundaries, between spaces, or as the whole input.
    // For example, "max" shouldn't match on content "our maximum effort",
    // but should match on content "our max effort".
    // Also, "(max)" should match on content "our (max) effort", even though
    // parentheses are not considered word characters.
    // https://github.com/Tyrrrz/DiscordChatExporter/issues/909
    private bool IsMatch(string? content) =>
        !string.IsNullOrWhiteSpace(content)
        && Regex.IsMatch(
            content,
            @"(?:\b|\s|^)" + Regex.Escape(text) + @"(?:\b|\s|$)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

    public override bool IsMatch(Message message) =>
        IsMatch(message.Content)
        || message.Embeds.Any(e =>
            IsMatch(e.Title)
            || IsMatch(e.Author?.Name)
            || IsMatch(e.Description)
            || IsMatch(e.Footer?.Text)
            || e.Fields.Any(f => IsMatch(f.Name) || IsMatch(f.Value))
        );
}
