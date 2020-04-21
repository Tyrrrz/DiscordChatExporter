using System.Collections.Generic;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Dialogs;

namespace DiscordChatExporter.Gui.ViewModels.Framework
{
    public static class Extensions
    {
        public static ChannelViewModel CreateChannelViewModel(this IViewModelFactory factory, Channel model, string? category = null)
        {
            var viewModel = factory.CreateChannelViewModel();
            viewModel.Model = model;
            viewModel.Category = category;

            return viewModel;
        }

        public static GuildViewModel CreateGuildViewModel(this IViewModelFactory factory, Guild model,
            IReadOnlyList<ChannelViewModel> channels)
        {
            var viewModel = factory.CreateGuildViewModel();
            viewModel.Model = model;
            viewModel.Channels = channels;

            return viewModel;
        }

        public static ExportSetupViewModel CreateExportSetupViewModel(this IViewModelFactory factory,
            GuildViewModel guild, IReadOnlyList<ChannelViewModel> channels)
        {
            var viewModel = factory.CreateExportSetupViewModel();
            viewModel.Guild = guild;
            viewModel.Channels = channels;

            return viewModel;
        }
    }
}