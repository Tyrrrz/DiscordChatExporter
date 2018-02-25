using System;
using System.Threading.Tasks;
using Onova;
using Onova.Services;

namespace DiscordChatExporter.Core.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly UpdateManager _updateManager;

        private Version _lastVersion;
        private bool _applied;

        public UpdateService()
        {
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

            // Remove some junk left over from last update
            _updateManager.Cleanup();

            // Check for updates
            var check = await _updateManager.CheckForUpdatesAsync();

            // Return latest version or null if running latest version already
            return check.CanUpdate ? _lastVersion = check.LastVersion : null;
        }

        public async Task PrepareUpdateAsync()
        {
            if (_lastVersion == null)
                return;

            // Download and prepare update
            await _updateManager.PreparePackageAsync(_lastVersion);
        }

        public async Task ApplyUpdateAsync(bool restart = true)
        {
            if (_lastVersion == null)
                return;
            if (_applied)
                return;

            // Enqueue an update
            await _updateManager.EnqueueApplyPackageAsync(_lastVersion, restart);

            _applied = true;
        }
    }
}