using System;
using System.Threading.Tasks;
using Onova;
using Onova.Services;

namespace DiscordChatExporter.Core.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly ISettingsService _settingsService;
        private readonly UpdateManager _updateManager;

        private Version _lastVersion;
        private bool _applied;

        public UpdateService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            _updateManager = new UpdateManager(
                new GithubPackageResolver("Tyrrrz", "DiscordChatExporter", "DiscordChatExporter.zip"),
                new ZipPackageExtractor());
        }

        public async Task<Version> CheckForUpdatesAsync()
        {
#if DEBUG
            // Never update in DEBUG mode
            return null;
#endif

            // Don't update if user disabled it
            if (!_settingsService.IsAutoUpdateEnabled)
                return null;

            try
            {
                // Remove some junk left over from last update
                _updateManager.Cleanup();

                // Check for updates
                var check = await _updateManager.CheckForUpdatesAsync();

                // Return latest version or null if running latest version already
                return check.CanUpdate ? _lastVersion = check.LastVersion : null;
            }
            catch
            {
                // It's okay for update to fail
                return null;
            }
        }

        public async Task PrepareUpdateAsync()
        {
            if (_lastVersion == null)
                return;

            try
            {
                // Download and prepare update
                await _updateManager.PreparePackageAsync(_lastVersion);
            }
            catch
            {
                // It's okay for update to fail
            }
        }

        public async Task ApplyUpdateAsync(bool restart = true)
        {
            if (_lastVersion == null)
                return;
            if (_applied)
                return;

            try
            {
                // Enqueue an update
                await _updateManager.EnqueueApplyPackageAsync(_lastVersion, restart);
                _applied = true;
            }
            catch
            {
                // It's okay for update to fail
            }
        }
    }
}