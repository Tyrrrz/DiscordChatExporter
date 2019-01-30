using DiscordChatExporter.Core.Models;
using Stylet;

namespace DiscordChatExporter.Gui.ViewModels.Components
{
    public class ChannelViewModel : PropertyChangedBase
    {
        public Channel Model { get; set; }

        public string Category { get; set; }
    }
}