using System.Collections.Generic;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Gui.ViewModels.Dialogs;

namespace DiscordChatExporter.Gui.ViewModels.Framework
{
    public static class Extensions
    {
        public static ExportSetupViewModel CreateExportSetupViewModel(this IViewModelFactory factory,
            Guild guild, IReadOnlyList<Channel> channels)
        {
            var viewModel = factory.CreateExportSetupViewModel();
            viewModel.Guild = guild;
            viewModel.Channels = channels;

            return viewModel;
        }
    }
}