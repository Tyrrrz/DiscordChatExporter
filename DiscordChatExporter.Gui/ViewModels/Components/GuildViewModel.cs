using System.Collections.Generic;
using DiscordChatExporter.Domain.Discord.Models;
using Stylet;

namespace DiscordChatExporter.Gui.ViewModels.Components
{
    public partial class GuildViewModel : PropertyChangedBase
    {
        public Guild? Model { get; set; }

        public IReadOnlyList<ChannelViewModel>? Channels { get; set; }
    }

    public partial class GuildViewModel
    {
        public static implicit operator Guild?(GuildViewModel? viewModel) => viewModel?.Model;
    }
}