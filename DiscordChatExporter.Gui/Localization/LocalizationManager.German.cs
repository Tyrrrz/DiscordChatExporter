using System.Collections.Generic;

namespace DiscordChatExporter.Gui.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> GermanLocalization = new Dictionary<
        string,
        string
    >
    {
        // Dashboard
        [nameof(PullGuildsTooltip)] = "Verfügbare Server und Kanäle laden (Enter)",
        [nameof(SettingsTooltip)] = "Einstellungen",
        [nameof(LastMessageSentTooltip)] = "Letzte Nachricht gesendet:",
        [nameof(TokenWatermark)] = "Token",
        // Token instructions (personal account)
        [nameof(TokenPersonalHeader)] = "Token für Ihr persönliches Konto abrufen:",
        [nameof(TokenPersonalTosWarning)] =
            "*  Das Automatisieren von Benutzerkonten verstößt technisch gegen die AGB —",
        [nameof(TokenPersonalTosRisk)] = "auf eigene Gefahr",
        [nameof(TokenPersonalStep1Before)] = "1. Öffnen Sie Discord in Ihrem",
        [nameof(TokenPersonalStep1After)] = "und melden Sie sich an",
        [nameof(TokenPersonalStep2)] =
            "2. Öffnen Sie einen Server oder einen direkten Nachrichtenkanal",
        [nameof(TokenPersonalStep3)] =
            "3. Drücken Sie Ctrl+Shift+I, um die Entwicklertools anzuzeigen",
        [nameof(TokenPersonalStep4)] = "4. Navigieren Sie zum Reiter Network",
        [nameof(TokenPersonalStep5)] = "5. Drücken Sie Ctrl+R zum Neuladen",
        [nameof(TokenPersonalStep6)] =
            "6. Wechseln Sie zwischen Kanälen, um Netzwerkanfragen auszulösen",
        [nameof(TokenPersonalStep7)] = "7. Suchen Sie nach einer Anfrage, die mit messages beginnt",
        [nameof(TokenPersonalStep8)] = "8. Wählen Sie den Reiter Headers auf der rechten Seite",
        [nameof(TokenPersonalStep9)] = "9. Scrollen Sie nach unten zum Abschnitt Request Headers",
        [nameof(TokenPersonalStep10)] = "10. Kopieren Sie den Wert des Headers authorization",
        [nameof(TokenWebBrowserLinkText)] = "Webbrowser",
        // Token instructions (bot)
        [nameof(TokenBotHeader)] = "Token für Ihren Bot abrufen:",
        [nameof(TokenBotIntro)] =
            "Der Token wird bei der Bot-Erstellung generiert. Falls er verloren gegangen ist, generieren Sie einen neuen:",
        [nameof(TokenBotStep1)] = "1. Öffnen Sie Discord",
        [nameof(TokenBotStep2)] = "2. Öffnen Sie die Einstellungen Ihrer Anwendung",
        [nameof(TokenBotStep3)] = "3. Navigieren Sie zum Abschnitt Bot auf der linken Seite",
        [nameof(TokenBotStep4)] = "4. Klicken Sie unter Token auf Reset Token",
        [nameof(TokenBotStep5)] = "5. Klicken Sie auf Yes, do it! und bestätigen Sie",
        [nameof(TokenBotStep6)] =
            "*  Integrationen, die den alten Token verwenden, hören auf zu funktionieren, bis sie aktualisiert werden",
        [nameof(TokenBotStep7Before)] = "*  Ihr Bot benötigt die aktivierte",
        [nameof(TokenBotStep7After)] = "um Nachrichten zu lesen",
        [nameof(TokenDeveloperPortalLinkText)] = "Entwicklerportal",
        [nameof(TokenDocumentationLinkText)] = "Dokumentation",
        [nameof(TokenHelpText)] = "Bei Fragen oder Problemen lesen Sie die",
        // Settings
        [nameof(SettingsTitle)] = "Einstellungen",
        [nameof(ThemeLabel)] = "Design",
        [nameof(ThemeTooltip)] = "Bevorzugtes Oberflächendesign",
        [nameof(LanguageLabel)] = "Sprache",
        [nameof(LanguageTooltip)] = "Bevorzugte Sprache der Benutzeroberfläche",
        [nameof(AutoUpdateLabel)] = "Automatische Updates",
        [nameof(AutoUpdateTooltip)] = "Automatische Updates bei jedem Start durchführen",
        [nameof(PersistTokenLabel)] = "Token speichern",
        [nameof(PersistTokenTooltip)] =
            "Den zuletzt verwendeten Token in einer Datei speichern, damit er zwischen Sitzungen erhalten bleibt",
        [nameof(RateLimitPreferenceLabel)] = "Ratenlimit-Einstellung",
        [nameof(RateLimitPreferenceTooltip)] =
            "Ob empfohlene Ratenlimits eingehalten werden sollen. Wenn deaktiviert, werden nur harte Ratenlimits (d. h. 429-Antworten) eingehalten.",
        [nameof(ShowThreadsLabel)] = "Threads anzeigen",
        [nameof(ShowThreadsTooltip)] = "Welche Thread-Typen in der Kanalliste angezeigt werden",
        [nameof(LocaleLabel)] = "Gebietsschema",
        [nameof(LocaleTooltip)] = "Gebietsschema für die Formatierung von Daten und Zahlen",
        [nameof(NormalizeToUtcLabel)] = "Auf UTC normalisieren",
        [nameof(NormalizeToUtcTooltip)] = "Alle Zeitstempel auf UTC+0 normalisieren",
        [nameof(ParallelLimitLabel)] = "Paralleles Limit",
        [nameof(ParallelLimitTooltip)] = "Wie viele Kanäle gleichzeitig exportiert werden können",
        // Export Setup
        [nameof(ChannelsSelectedText)] = "Kanäle ausgewählt",
        [nameof(OutputPathLabel)] = "Ausgabepfad",
        [nameof(FormatLabel)] = "Format",
        [nameof(FormatTooltip)] = "Exportformat",
        [nameof(AfterDateLabel)] = "Nach (Datum)",
        [nameof(AfterDateTooltip)] =
            "Nur Nachrichten einschließen, die nach diesem Datum gesendet wurden",
        [nameof(BeforeDateLabel)] = "Vor (Datum)",
        [nameof(BeforeDateTooltip)] =
            "Nur Nachrichten einschließen, die vor diesem Datum gesendet wurden",
        [nameof(AfterTimeLabel)] = "Nach (Uhrzeit)",
        [nameof(AfterTimeTooltip)] =
            "Nur Nachrichten einschließen, die nach dieser Uhrzeit gesendet wurden",
        [nameof(BeforeTimeLabel)] = "Vor (Uhrzeit)",
        [nameof(BeforeTimeTooltip)] =
            "Nur Nachrichten einschließen, die vor dieser Uhrzeit gesendet wurden",
        [nameof(PartitionLimitLabel)] = "Partitionslimit",
        [nameof(PartitionLimitTooltip)] =
            "Die Ausgabe in Partitionen aufteilen, jede begrenzt auf die angegebene Anzahl von Nachrichten (z. B. '100') oder Dateigröße (z. B. '10mb')",
        [nameof(MessageFilterLabel)] = "Nachrichtenfilter",
        [nameof(MessageFilterTooltip)] =
            "Nur Nachrichten einschließen, die diesem Filter entsprechen (z. B. 'from:foo#1234' oder 'has:image'). Weitere Informationen finden Sie in der Dokumentation.",
        [nameof(FormatMarkdownLabel)] = "Markdown formatieren",
        [nameof(FormatMarkdownTooltip)] =
            "Markdown, Erwähnungen und andere spezielle Token verarbeiten",
        [nameof(DownloadAssetsLabel)] = "Assets herunterladen",
        [nameof(DownloadAssetsTooltip)] =
            "Vom Export referenzierte Assets herunterladen (Benutzeravatare, angehängte Dateien, eingebettete Bilder usw.)",
        [nameof(ReuseAssetsLabel)] = "Assets wiederverwenden",
        [nameof(ReuseAssetsTooltip)] =
            "Zuvor heruntergeladene Assets wiederverwenden, um redundante Anfragen zu vermeiden",
        [nameof(AssetsDirPathLabel)] = "Asset-Verzeichnispfad",
        [nameof(AssetsDirPathTooltip)] =
            "Assets in dieses Verzeichnis herunterladen. Wenn nicht angegeben, wird der Asset-Verzeichnispfad vom Ausgabepfad abgeleitet.",
        [nameof(AdvancedOptionsTooltip)] = "Erweiterte Optionen umschalten",
        [nameof(ExportButton)] = "EXPORTIEREN",
        // Common buttons
        [nameof(CloseButton)] = "SCHLIESSEN",
        [nameof(CancelButton)] = "ABBRECHEN",
        // Dialog messages
        [nameof(UkraineSupportTitle)] = "Danke für Ihre Unterstützung der Ukraine!",
        [nameof(UkraineSupportMessage)] = """
            Während Russland einen Vernichtungskrieg gegen mein Land führt, bin ich jedem dankbar, der weiterhin an der Seite der Ukraine in unserem Kampf für die Freiheit steht.

            Klicken Sie auf MEHR ERFAHREN, um Möglichkeiten der Unterstützung zu finden.
            """,
        [nameof(LearnMoreButton)] = "MEHR ERFAHREN",
        [nameof(UnstableBuildTitle)] = "Warnung: Instabile Version",
        [nameof(UnstableBuildMessage)] = """
            Sie verwenden eine Entwicklungsversion von {0}. Diese Versionen wurden nicht gründlich getestet und können Fehler enthalten.

            Automatische Updates sind für Entwicklungsversionen deaktiviert.

            Klicken Sie auf RELEASES ANZEIGEN, wenn Sie stattdessen eine stabile Version herunterladen möchten.
            """,
        [nameof(SeeReleasesButton)] = "RELEASES ANZEIGEN",
        [nameof(UpdateDownloadingMessage)] = "Update auf {0} v{1} wird heruntergeladen...",
        [nameof(UpdateReadyMessage)] =
            "Update wurde heruntergeladen und wird beim Beenden installiert",
        [nameof(UpdateInstallNowButton)] = "JETZT INSTALLIEREN",
        [nameof(UpdateFailedMessage)] = "Anwendungsupdate konnte nicht durchgeführt werden",
        [nameof(ErrorPullingServersTitle)] = "Fehler beim Laden der Server",
        [nameof(ErrorPullingChannelsTitle)] = "Fehler beim Laden der Kanäle",
        [nameof(ErrorExportingTitle)] = "Fehler beim Exportieren der Kanäle",
        [nameof(SuccessfulExportMessage)] = "{0} Kanal/-äle erfolgreich exportiert",
    };
}
