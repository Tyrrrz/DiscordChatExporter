using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Utils.Logging;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace DiscordChatExporter.Gui.Services;

public class UpdateService(SettingsService settingsService) : IDisposable
{
    private readonly IUpdateManager? _updateManager =
        OperatingSystem.IsWindows() && StartOptions.Current.IsAutoUpdateAllowed
            ? new UpdateManager(
                new GithubPackageResolver(
                    "Tyrrrz",
                    "DiscordChatExporter",
                    // Examples:
                    // DiscordChatExporter.win-arm64.zip
                    // DiscordChatExporter.win-x64.zip
                    // DiscordChatExporter.linux-x64.zip
                    $"DiscordChatExporter.{RuntimeInformation.RuntimeIdentifier}.zip"
                ),
                new ZipPackageExtractor()
            )
            : null;

    private Version? _updateVersion;
    private bool _isUpdatePrepared;
    private bool _isUpdaterLaunched;

    public async ValueTask<Version?> CheckForUpdatesAsync()
    {
        if (_updateManager is null)
            return null;

        if (!settingsService.IsAutoUpdateEnabled)
            return null;

        Logger.Info(LogCategory.App, "Checking for application updates...");

        var check = await _updateManager.CheckForUpdatesAsync();
        if (check.CanUpdate && check.LastVersion is not null)
        {
            Logger.Info(
                LogCategory.App,
                $"New application version available: v{check.LastVersion}."
            );
            return check.LastVersion;
        }

        Logger.Info(LogCategory.App, "Application is up to date.");
        return null;
    }

    public async ValueTask PrepareUpdateAsync(Version version)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        try
        {
            Logger.Info(LogCategory.App, $"Preparing update v{version}...");
            await _updateManager.PrepareUpdateAsync(_updateVersion = version);
            _isUpdatePrepared = true;
            Logger.Info(LogCategory.App, $"Update v{version} prepared and ready to install.");
        }
        catch (UpdaterAlreadyLaunchedException ex)
        {
            Logger.Warning(
                LogCategory.App,
                $"Skipped preparing update v{version}: updater was already launched ({ex.GetType().Name})."
            );
        }
        catch (LockFileNotAcquiredException ex)
        {
            Logger.Warning(
                LogCategory.App,
                $"Skipped preparing update v{version}: update lock file could not be acquired ({ex.GetType().Name})."
            );
        }
    }

    public void FinalizeUpdate(bool needRestart)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        if (_updateVersion is null || !_isUpdatePrepared || _isUpdaterLaunched)
            return;

        try
        {
            _updateManager.LaunchUpdater(_updateVersion, needRestart);
            _isUpdaterLaunched = true;
            Logger.Info(
                LogCategory.App,
                $"Launched updater for v{_updateVersion} (Restart: {needRestart})."
            );
        }
        catch (UpdaterAlreadyLaunchedException ex)
        {
            Logger.Warning(
                LogCategory.App,
                $"Skipped launching updater for v{_updateVersion}: updater was already launched ({ex.GetType().Name})."
            );
        }
        catch (LockFileNotAcquiredException ex)
        {
            Logger.Warning(
                LogCategory.App,
                $"Skipped launching updater for v{_updateVersion}: update lock file could not be acquired ({ex.GetType().Name})."
            );
        }
    }

    public void Dispose() => _updateManager?.Dispose();
}
