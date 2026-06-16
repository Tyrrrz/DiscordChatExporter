using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Gui.Localization;
using DiscordChatExporter.Gui.ViewModels;
using DiscordChatExporter.Gui.ViewModels.Components;
using DiscordChatExporter.Gui.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordChatExporter.Gui.Framework;

public class ViewModelManager(IServiceProvider services, LocalizationManager localizationManager)
{
    public MainViewModel GetMainViewModel() => services.GetRequiredService<MainViewModel>();

    public DashboardViewModel GetDashboardViewModel() =>
        services.GetRequiredService<DashboardViewModel>();

    public ExportSetupViewModel GetExportSetupViewModel(
        Guild guild,
        IReadOnlyList<Channel> channels
    )
    {
        var viewModel = services.GetRequiredService<ExportSetupViewModel>();

        viewModel.Guild = guild;
        viewModel.Channels = channels;

        return viewModel;
    }

    public MessageBoxViewModel GetMessageBoxViewModel(
        string title,
        string message,
        string? okButtonText,
        string? cancelButtonText
    )
    {
        var viewModel = services.GetRequiredService<MessageBoxViewModel>();

        viewModel.Title = title;
        viewModel.Message = message;
        viewModel.DefaultButtonText = okButtonText;
        viewModel.CancelButtonText = cancelButtonText;

        return viewModel;
    }

    public MessageBoxViewModel GetMessageBoxViewModel(string title, string message) =>
        GetMessageBoxViewModel(title, message, localizationManager.CloseButton, null);

    public SettingsViewModel GetSettingsViewModel() =>
        services.GetRequiredService<SettingsViewModel>();
}
