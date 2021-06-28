using DiscordChatExporter.Core.Discord.Data;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class ContainsMessageFilter : MessageFilter
    {
        private readonly string _value;

        public ContainsMessageFilter(string value) => _value = value;

        public override bool Filter(Message message) => Regex.IsMatch(message.Content, $@"\b{Regex.Escape(_value)}\b", RegexOptions.IgnoreCase);
    }
}