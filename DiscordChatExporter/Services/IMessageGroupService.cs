using System.Collections.Generic;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IMessageGroupService
    {
        IEnumerable<MessageGroup> GroupMessages(IEnumerable<Message> messages);
    }
}