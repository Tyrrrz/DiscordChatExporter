using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class FromMessageFilter : MessageFilter
    {
        private readonly string _value;

        public FromMessageFilter(string value)
        {
            ///TODO: should we validate input here?
            _value = value;
        }

            //match either a username + discriminator or an id
        public override bool Filter(Message message) =>
            _value == message.Author.FullName || _value == message.Author.Id.ToString();
    }
}