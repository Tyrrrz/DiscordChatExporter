﻿using System.Collections.Generic;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public interface IMessageGroupService
    {
        IReadOnlyList<MessageGroup> GroupMessages(IEnumerable<Message> messages);
    }
}