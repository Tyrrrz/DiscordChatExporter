using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class FromMessageFilter : MessageFilter
    {
        private readonly string _value;

        public FromMessageFilter(string value) => _value = value;

        public override bool Filter(Message message) =>
            _value == message.Author.Name ||
            _value == message.Author.FullName ||
            _value == message.Author.Id.ToString();
    }
}