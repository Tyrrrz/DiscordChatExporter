using System.Collections.Generic;
using DiscordChatExporter.Core.Models;
using Stylet;

namespace DiscordChatExporter.Gui.ViewModels.Components
{
    public class GuildViewModel : PropertyChangedBase
    {
        public Guild Model { get; set; }

        public IReadOnlyList<ChannelViewModel> Channels { get; set; }
    }
}