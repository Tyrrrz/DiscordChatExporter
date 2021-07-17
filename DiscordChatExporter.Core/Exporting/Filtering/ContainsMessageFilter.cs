using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    internal class ContainsMessageFilter : MessageFilter
    {
        private readonly string _text;

        public ContainsMessageFilter(string text) => _text = text;

        public override bool Filter(Message message) => Regex.IsMatch(
            message.Content,
            "\\b" + _text + "\\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );
    }
}