using DiscordChatExporter.Core.Discord.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class HasMessageFilter : MessageFilter
    {
        private readonly string _value;

        public HasMessageFilter(string value) => _value = value;

        public override bool Filter(Message message) =>
            _value switch
            {
                "link" => Regex.IsMatch(message.Content, "https?://\\S*[^\\.,:;\"\'\\s]", DefaultRegexOptions),
                "embed" => message.Embeds.Any(),
                "file" => message.Attachments.Any(),
                "video" => message.Attachments.Any(file => file.IsVideo),
                "image" => message.Attachments.Any(file => file.IsImage),
                "sound" => message.Attachments.Any(file => file.IsAudio),
                _ => throw new InvalidOperationException($"Invalid value provided for the 'has' message filter: '{_value}'")
            };
    }
}
