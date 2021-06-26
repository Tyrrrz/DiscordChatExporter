using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class ContainsMessageFilter : MessageFilter
    {
        private readonly string _value;

        public ContainsMessageFilter(string value)
        {
            _value = value;
        }

        public override bool Filter(Message message) => message.Content.Contains(_value);
    }
}