using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class FromMessageFilter : MessageFilter
    {
        private string _value;

        public FromMessageFilter(string value)
        {
            ///TODO: should we validate input here?
            _value = value;
        }

        public override bool Filter(Message message)
        {
            //match either a username + discriminator or an id
            return _value == message.Author.FullName || _value == message.Author.Id.ToString();
        }
    }
}