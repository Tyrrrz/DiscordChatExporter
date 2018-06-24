using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public class MessageGroupService : IMessageGroupService
    {
        private readonly ISettingsService _settingsService;

        public MessageGroupService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public IReadOnlyList<MessageGroup> GroupMessages(IEnumerable<Message> messages)
        {
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
                        (message.Timestamp - groupFirst.Timestamp).TotalHours > 1 ||
                        message.Timestamp.Hour != groupFirst.Timestamp.Hour ||
                        groupBuffer.Count >= _settingsService.MessageGroupLimit
                    );

                // If condition is true - flush buffer
                if (breakCondition)
                {
                    var group = new MessageGroup(groupFirst.Author, groupFirst.Timestamp, groupBuffer.ToArray());
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
                var group = new MessageGroup(groupFirst.Author, groupFirst.Timestamp, groupBuffer.ToArray());
                result.Add(group);
            }

            return result;
        }
    }
}