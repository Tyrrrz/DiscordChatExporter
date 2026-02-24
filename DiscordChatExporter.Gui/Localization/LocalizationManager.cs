using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordChatExporter.Gui.Services;
using DiscordChatExporter.Gui.Utils;
using DiscordChatExporter.Gui.Utils.Extensions;

namespace DiscordChatExporter.Gui.Localization;

public partial class LocalizationManager : ObservableObject, IDisposable
{
    private readonly DisposableCollector _eventRoot = new();

    public LocalizationManager(SettingsService settingsService)
    {
        _eventRoot.Add(
            settingsService.WatchProperty(
                o => o.Language,
                () => Language = settingsService.Language,
                true
            )
        );

        _eventRoot.Add(
            this.WatchProperty(
                o => o.Language,
                () =>
                {
                    foreach (var propertyName in EnglishLocalization.Keys)
                        OnPropertyChanged(propertyName);
                }
            )
        );
    }

    [ObservableProperty]
    public partial Language Language { get; set; } = Language.System;

    private string Get([CallerMemberName] string? key = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        var localization = Language switch
        {
            Language.System =>
                CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName.ToLowerInvariant() switch
                {
                    "ukr" => UkrainianLocalization,
                    "deu" => GermanLocalization,
                    "fra" => FrenchLocalization,
                    "spa" => SpanishLocalization,
                    _ => EnglishLocalization,
                },
            Language.Ukrainian => UkrainianLocalization,
            Language.German => GermanLocalization,
            Language.French => FrenchLocalization,
            Language.Spanish => SpanishLocalization,
            _ => EnglishLocalization,
        };

        if (
            localization.TryGetValue(key, out var value)
            // English is used as a fallback
            || EnglishLocalization.TryGetValue(key, out value)
        )
        {
            return value;
        }

        return $"Missing localization for '{key}'";
    }

    public void Dispose() => _eventRoot.Dispose();
}

public partial class LocalizationManager
{
    // ---- Dashboard ----

    public string PullGuildsTooltip => Get();
    public string SettingsTooltip => Get();
    public string LastMessageSentTooltip => Get();
    public string TokenWatermark => Get();

    // Token instructions (personal account)
    public string TokenPersonalHeader => Get();
    public string TokenPersonalTosWarning => Get();
    public string TokenPersonalTosRisk => Get();
    public string TokenPersonalStep1Before => Get();
    public string TokenPersonalStep1After => Get();
    public string TokenPersonalStep2 => Get();
    public string TokenPersonalStep3 => Get();
    public string TokenPersonalStep4 => Get();
    public string TokenPersonalStep5 => Get();
    public string TokenPersonalStep6 => Get();
    public string TokenPersonalStep7 => Get();
    public string TokenPersonalStep8 => Get();
    public string TokenPersonalStep9 => Get();
    public string TokenPersonalStep10 => Get();
    public string TokenWebBrowserLinkText => Get();

    // Token instructions (bot)
    public string TokenBotHeader => Get();
    public string TokenBotIntro => Get();
    public string TokenBotStep1 => Get();
    public string TokenBotStep2 => Get();
    public string TokenBotStep3 => Get();
    public string TokenBotStep4 => Get();
    public string TokenBotStep5 => Get();
    public string TokenBotStep6 => Get();
    public string TokenBotStep7Before => Get();
    public string TokenBotStep7After => Get();
    public string TokenDeveloperPortalLinkText => Get();
    public string TokenDocumentationLinkText => Get();
    public string TokenHelpText => Get();

    // ---- Settings ----

    public string SettingsTitle => Get();
    public string ThemeLabel => Get();
    public string ThemeTooltip => Get();
    public string LanguageLabel => Get();
    public string LanguageTooltip => Get();
    public string AutoUpdateLabel => Get();
    public string AutoUpdateTooltip => Get();
    public string PersistTokenLabel => Get();
    public string PersistTokenTooltip => Get();
    public string RateLimitPreferenceLabel => Get();
    public string RateLimitPreferenceTooltip => Get();
    public string ShowThreadsLabel => Get();
    public string ShowThreadsTooltip => Get();
    public string LocaleLabel => Get();
    public string LocaleTooltip => Get();
    public string NormalizeToUtcLabel => Get();
    public string NormalizeToUtcTooltip => Get();
    public string ParallelLimitLabel => Get();
    public string ParallelLimitTooltip => Get();

    // ---- Export Setup ----

    public string ChannelsSelectedText => Get();
    public string OutputPathLabel => Get();
    public string FormatLabel => Get();
    public string FormatTooltip => Get();
    public string AfterDateLabel => Get();
    public string AfterDateTooltip => Get();
    public string BeforeDateLabel => Get();
    public string BeforeDateTooltip => Get();
    public string AfterTimeLabel => Get();
    public string AfterTimeTooltip => Get();
    public string BeforeTimeLabel => Get();
    public string BeforeTimeTooltip => Get();
    public string PartitionLimitLabel => Get();
    public string PartitionLimitTooltip => Get();
    public string MessageFilterLabel => Get();
    public string MessageFilterTooltip => Get();
    public string FormatMarkdownLabel => Get();
    public string FormatMarkdownTooltip => Get();
    public string DownloadAssetsLabel => Get();
    public string DownloadAssetsTooltip => Get();
    public string ReuseAssetsLabel => Get();
    public string ReuseAssetsTooltip => Get();
    public string AssetsDirPathLabel => Get();
    public string AssetsDirPathTooltip => Get();
    public string AdvancedOptionsTooltip => Get();
    public string ExportButton => Get();

    // ---- Common buttons ----

    public string CloseButton => Get();
    public string CancelButton => Get();

    // ---- Dialog messages ----

    public string UkraineSupportTitle => Get();
    public string UkraineSupportMessage => Get();
    public string LearnMoreButton => Get();
    public string UnstableBuildTitle => Get();
    public string UnstableBuildMessage => Get();
    public string SeeReleasesButton => Get();
    public string UpdateDownloadingMessage => Get();
    public string UpdateReadyMessage => Get();
    public string UpdateInstallNowButton => Get();
    public string UpdateFailedMessage => Get();
    public string ErrorPullingServersTitle => Get();
    public string ErrorPullingChannelsTitle => Get();
    public string ErrorExportingTitle => Get();
    public string SuccessfulExportMessage => Get();
}
