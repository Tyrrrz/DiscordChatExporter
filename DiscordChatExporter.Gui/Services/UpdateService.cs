using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace DiscordChatExporter.Gui.Services;

public class UpdateService(SettingsService settingsService) : IDisposable
{
    private readonly IUpdateManager? _updateManager = OperatingSystem.IsWindows()
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
    private bool _updatePrepared;
    private bool _updaterLaunched;

    public async ValueTask<Version?> CheckForUpdatesAsync()
    {
        if (_updateManager is null)
            return null;

        if (!settingsService.IsAutoUpdateEnabled)
            return null;

        var check = await _updateManager.CheckForUpdatesAsync();
        return check.CanUpdate ? check.LastVersion : null;
    }

    public async ValueTask PrepareUpdateAsync(Version version)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        try
        {
            await _updateManager.PrepareUpdateAsync(_updateVersion = version);
            _updatePrepared = true;
        }
        catch (UpdaterAlreadyLaunchedException)
        {
            // Ignore race conditions
        }
        catch (LockFileNotAcquiredException)
        {
            // Ignore race conditions
        }
    }

    public void FinalizeUpdate(bool needRestart)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        if (_updateVersion is null || !_updatePrepared || _updaterLaunched)
            return;

        try
        {
            _updateManager.LaunchUpdater(_updateVersion, needRestart);
            _updaterLaunched = true;
        }
        catch (UpdaterAlreadyLaunchedException)
        {
            // Ignore race conditions
        }
        catch (LockFileNotAcquiredException)
        {
            // Ignore race conditions
        }
    }

    public void Dispose() => _updateManager?.Dispose();
}
