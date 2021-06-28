using DiscordChatExporter.Core.Discord.Data;
using System.Linq;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class HasMessageFilter : MessageFilter
    {
        private readonly string _value;

        public HasMessageFilter(string value) => _value = value;

        public override bool Filter(Message message) =>
            _value switch
            {
                "link" => throw new System.NotImplementedException(), //how do we do this?
                "embed" => message.Embeds.Any(),
                "file" => message.Attachments.Any(),
                "video" => message.Attachments.Any(file => file.IsVideo),
                "image" => message.Attachments.Any(file => file.IsImage),
                "sound" => message.Attachments.Any(file => file.IsAudio),
                _ => false
            };
    }
}