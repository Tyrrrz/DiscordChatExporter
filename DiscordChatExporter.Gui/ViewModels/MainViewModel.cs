using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.Utils;
using DiscordChatExporter.Gui.Utils.Extensions;
using DiscordChatExporter.Gui.ViewModels.Components;

namespace DiscordChatExporter.Gui.ViewModels;

public partial class MainViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SnackbarManager snackbarManager,
    SettingsService settingsService,
    UpdateService updateService
) : ViewModelBase
{
    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; } = viewModelManager.CreateDashboardViewModel();

    private async Task ShowUkraineSupportMessageAsync()
    {
        if (!settingsService.IsUkraineSupportMessageEnabled)
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            "Thank you for supporting Ukraine!",
            """
            As Russia wages a genocidal war against my country, I'm grateful to everyone who continues to stand with Ukraine in our fight for freedom.

            Click LEARN MORE to find ways that you can help.
            """,
            "LEARN MORE",
            "CLOSE"
        );

        // Disable this message in the future
        settingsService.IsUkraineSupportMessageEnabled = false;
        settingsService.Save();

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            ProcessEx.StartShellExecute("https://tyrrrz.me/ukraine?source=discordchatexporter");
    }

    private async Task ShowDevelopmentBuildMessageAsync()
    {
        if (!Program.IsDevelopmentBuild)
            return;

        // If debugging, the user is likely a developer
        if (Debugger.IsAttached)
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            "Unstable build warning",
            """
            You're using a development build of the application. These builds are not thoroughly tested and may contain bugs.

            Auto-updates are disabled for development builds. If you want to switch to a stable release, please download it manually.
            """,
            "SEE RELEASES",
            "CLOSE"
        );

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            ProcessEx.StartShellExecute(Program.ProjectReleasesUrl);
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateVersion = await updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            snackbarManager.Notify($"Downloading update to {Program.Name} v{updateVersion}...");
            await updateService.PrepareUpdateAsync(updateVersion);

            snackbarManager.Notify(
                "Update has been downloaded and will be installed when you exit",
                "INSTALL NOW",
                () =>
                {
                    updateService.FinalizeUpdate(true);

                    if (Application.Current?.ApplicationLifetime?.TryShutdown(2) != true)
                        Environment.Exit(2);
                }
            );
        }
        catch
        {
            // Failure to update shouldn't crash the application
            snackbarManager.Notify("Failed to perform application update");
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowUkraineSupportMessageAsync();
        await ShowDevelopmentBuildMessageAsync();
        await CheckForUpdatesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Save settings
            settingsService.Save();

            // Finalize pending updates
            updateService.FinalizeUpdate(false);
        }

        base.Dispose(disposing);
    }
}
