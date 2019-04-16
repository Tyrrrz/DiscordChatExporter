using System;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Services;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace DiscordChatExporter.Gui.Services
{
    public class UpdateService : IDisposable
    {
        private readonly SettingsService _settingsService;

        private readonly IUpdateManager _updateManager = new UpdateManager(
            new GithubPackageResolver("Tyrrrz", "DiscordChatExporter", "DiscordChatExporter.zip"),
            new ZipPackageExtractor());

        private Version _updateVersion;
        private bool _updaterLaunched;

        public UpdateService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<Version> CheckPrepareUpdateAsync()
        {
            try
            {
                // If auto-update is disabled - don't check for updates
                if (!_settingsService.IsAutoUpdateEnabled)
                    return null;

                // Check for updates
                var check = await _updateManager.CheckForUpdatesAsync();
                if (!check.CanUpdate)
                    return null;

                // Prepare the update
                await _updateManager.PrepareUpdateAsync(check.LastVersion);

                return _updateVersion = check.LastVersion;
            }
            catch (UpdaterAlreadyLaunchedException)
            {
                return null;
            }
            catch (LockFileNotAcquiredException)
            {
                return null;
            }
        }

        public void FinalizeUpdate(bool needRestart)
        {
            try
            {
                // Check if an update is pending
                if (_updateVersion == null)
                    return;

                // Check if the updater has already been launched
                if (_updaterLaunched)
                    return;

                // Launch the updater
                _updateManager.LaunchUpdater(_updateVersion, needRestart);
                _updaterLaunched = true;
            }
            catch (UpdaterAlreadyLaunchedException)
            {
            }
            catch (LockFileNotAcquiredException)
            {
            }
        }

        public void Dispose() => _updateManager.Dispose();
    }
}