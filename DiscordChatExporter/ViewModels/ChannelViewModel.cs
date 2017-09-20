using DiscordChatExporter.Models;

namespace DiscordChatExporter.ViewModels
{
    public class ChannelViewModel
    {
        public string GroupName { get; }

        public Channel Channel { get; }

        public ChannelViewModel(string groupName, Channel channel)
        {
            GroupName = groupName;
            Channel = channel;
        }
    }
}