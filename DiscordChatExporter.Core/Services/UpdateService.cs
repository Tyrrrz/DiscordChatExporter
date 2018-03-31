using System;
using System.Threading.Tasks;
using Onova;
using Onova.Services;

namespace DiscordChatExporter.Core.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly ISettingsService _settingsService;
        private readonly IUpdateManager _manager;

        private Version _updateVersion;
        private bool _updateFinalized;

        public bool NeedRestart { get; set; }

        public UpdateService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            _manager = new UpdateManager(
                new GithubPackageResolver("Tyrrrz", "DiscordChatExporter", "DiscordChatExporter.zip"),
                new ZipPackageExtractor());
        }

        public async Task<Version> CheckPrepareUpdateAsync()
        {
            // If auto-update is disabled - don't check for updates
            if (!_settingsService.IsAutoUpdateEnabled)
                return null;

            // Cleanup leftover files
            _manager.Cleanup();

            // Check for updates
            var check = await _manager.CheckForUpdatesAsync();
            if (!check.CanUpdate)
                return null;

            // Prepare the update
            if (!_manager.IsUpdatePrepared(check.LastVersion))
                await _manager.PrepareUpdateAsync(check.LastVersion);

            return _updateVersion = check.LastVersion;
        }

        public void FinalizeUpdate()
        {
            // Check if an update is pending
            if (_updateVersion == null)
                return;

            // Check if the update has already been finalized
            if (_updateFinalized)
                return;

            // Launch the updater
            _manager.LaunchUpdater(_updateVersion, NeedRestart);
            _updateFinalized = true;
        }
    }
}