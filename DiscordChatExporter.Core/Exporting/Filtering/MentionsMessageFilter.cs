using DiscordChatExporter.Core.Discord.Data;
using System.Linq;

namespace DiscordChatExporter.Core.Exporting.Filtering
{
    public class MentionsMessageFilter : MessageFilter
    {
        private readonly string _value;

        public MentionsMessageFilter(string value)
        {
            ///TODO: should we validate input here?
            _value = value;
        }

        //match either a username + discriminator or an id
        public override bool Filter(Message message) =>
            message.MentionedUsers.Any(user => _value == user.FullName || _value == user.Id.ToString());
    }
}