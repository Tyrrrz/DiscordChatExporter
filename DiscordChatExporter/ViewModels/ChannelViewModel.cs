using DiscordChatExporter.Models;

namespace DiscordChatExporter.ViewModels
{
    public class ChannelViewModel
    {
        public string Group { get; }

        public Channel Channel { get; }

        public ChannelViewModel(string group, Channel channel)
        {
            Group = group;
            Channel = channel;
        }
    }
}