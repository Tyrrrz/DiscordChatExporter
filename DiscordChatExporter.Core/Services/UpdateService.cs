using System;
using System.Threading.Tasks;
using Onova;
using Onova.Services;

namespace DiscordChatExporter.Core.Services
{
    public class UpdateService
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
            // If auto-update is disabled - don't check for updates
            if (!_settingsService.IsAutoUpdateEnabled)
                return null;

            // Cleanup leftover files
            _updateManager.Cleanup();

            // Check for updates
            var check = await _updateManager.CheckForUpdatesAsync();
            if (!check.CanUpdate)
                return null;

            // Prepare the update
            if (!_updateManager.IsUpdatePrepared(check.LastVersion))
                await _updateManager.PrepareUpdateAsync(check.LastVersion);

            return _updateVersion = check.LastVersion;
        }

        public void FinalizeUpdate(bool needRestart)
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
    }
}