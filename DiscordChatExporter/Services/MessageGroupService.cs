using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Models;

namespace DiscordChatExporter.Services
{
    public class MessageGroupService : IMessageGroupService
    {
        private readonly ISettingsService _settingsService;

        public MessageGroupService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public IReadOnlyList<MessageGroup> GroupMessages(IReadOnlyList<Message> messages)
        {
            var groupLimit = _settingsService.MessageGroupLimit;
            var result = new List<MessageGroup>();

            // Group adjacent messages by timestamp and author
            var groupBuffer = new List<Message>();
            foreach (var message in messages)
            {
                var groupFirst = groupBuffer.FirstOrDefault();

                // Group break condition
                var breakCondition =
                    groupFirst != null &&
                    (
                        message.Author.Id != groupFirst.Author.Id ||
                        (message.TimeStamp - groupFirst.TimeStamp).TotalHours > 1 ||
                        message.TimeStamp.Hour != groupFirst.TimeStamp.Hour ||
                        groupBuffer.Count >= groupLimit
                    );

                // If condition is true - flush buffer
                if (breakCondition)
                {
                    var group = new MessageGroup(groupFirst.Author, groupFirst.TimeStamp, groupBuffer.ToArray());
                    result.Add(group);
                    groupBuffer.Clear();
                }

                // Add message to buffer
                groupBuffer.Add(message);
            }

            // Add what's remaining in buffer
            if (groupBuffer.Any())
            {
                var groupFirst = groupBuffer.First();
                var group = new MessageGroup(groupFirst.Author, groupFirst.TimeStamp, groupBuffer.ToArray());
                result.Add(group);
            }

            return result;
        }
    }
}