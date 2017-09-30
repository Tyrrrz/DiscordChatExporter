using System.Collections.Generic;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public interface IMessageGroupService
    {
        IReadOnlyList<MessageGroup> GroupMessages(IReadOnlyList<Message> messages);
    }
}