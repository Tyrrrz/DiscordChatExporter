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
        private readonly IUpdateManager _updateManager = new UpdateManager(
            new GithubPackageResolver("Tyrrrz", "DiscordChatExporter", "DiscordChatExporter.zip"),
            new ZipPackageExtractor());

        private readonly SettingsService _settingsService;

        private Version? _updateVersion;
        private bool _updaterLaunched;

        public UpdateService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<Version?> CheckForUpdatesAsync()
        {
            if (!_settingsService.IsAutoUpdateEnabled)
                return null;

            var check = await _updateManager.CheckForUpdatesAsync();
            return check.CanUpdate ? check.LastVersion : null;
        }

        public async Task PrepareUpdateAsync(Version version)
        {
            if (!_settingsService.IsAutoUpdateEnabled)
                return;

            try
            {
                await _updateManager.PrepareUpdateAsync(_updateVersion = version);
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
            if (!_settingsService.IsAutoUpdateEnabled)
                return;

            if (_updateVersion == null || _updaterLaunched)
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

        public void Dispose() => _updateManager.Dispose();
    }
}