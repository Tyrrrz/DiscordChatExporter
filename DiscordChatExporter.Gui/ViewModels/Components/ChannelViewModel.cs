using DiscordChatExporter.Core.Models;
using Stylet;

namespace DiscordChatExporter.Gui.ViewModels.Components
{
    public partial class ChannelViewModel : PropertyChangedBase
    {
        public Channel? Model { get; set; }

        public string? Category { get; set; }
    }

    public partial class ChannelViewModel
    {
        public static implicit operator Channel?(ChannelViewModel? viewModel) => viewModel?.Model;
    }
}