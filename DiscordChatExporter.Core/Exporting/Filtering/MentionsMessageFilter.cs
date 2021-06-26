using DiscordChatExporter.Core.Discord.Data;
using System.Linq;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class MentionsMessageFilter : MessageFilter
    {
        private string _value;

        public MentionsMessageFilter(string value)
        {
            ///TODO: should we validate input here?
            _value = value;
        }

        public override bool Filter(Message message)
        {
            //match either a username + discriminator or an id
            return message.MentionedUsers.Any(user => _value == user.FullName || _value == user.Id.ToString());
        }
    }
}