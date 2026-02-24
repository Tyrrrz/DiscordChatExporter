using System.Collections.Generic;

namespace DiscordChatExporter.Gui.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> UkrainianLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(PullGuildsTooltip)] = "Завантажити доступні сервери та канали (Enter)",
            [nameof(SettingsTooltip)] = "Налаштування",
            [nameof(LastMessageSentTooltip)] = "Останнє повідомлення:",
            // Settings
            [nameof(SettingsTitle)] = "Налаштування",
            [nameof(ThemeLabel)] = "Тема",
            [nameof(ThemeTooltip)] = "Бажана тема інтерфейсу",
            [nameof(LanguageLabel)] = "Мова",
            [nameof(LanguageTooltip)] = "Бажана мова інтерфейсу",
            [nameof(AutoUpdateLabel)] = "Авто-оновлення",
            [nameof(AutoUpdateTooltip)] = "Виконувати автоматичні оновлення при кожному запуску",
            [nameof(PersistTokenLabel)] = "Зберігати токен",
            [nameof(PersistTokenTooltip)] =
                "Зберігати останній використаний токен у файлі для збереження між сеансами",
            [nameof(RateLimitPreferenceLabel)] = "Ліміт запитів",
            [nameof(RateLimitPreferenceTooltip)] =
                "Чи дотримуватись рекомендованих лімітів запитів. Якщо вимкнено, будуть дотримуватись лише жорсткі ліміти (тобто відповіді 429).",
            [nameof(ShowThreadsLabel)] = "Показувати гілки",
            [nameof(ShowThreadsTooltip)] = "Які типи гілок показувати у списку каналів",
            [nameof(LocaleLabel)] = "Локаль",
            [nameof(LocaleTooltip)] = "Локаль для форматування дат та чисел",
            [nameof(NormalizeToUtcLabel)] = "Нормалізувати до UTC",
            [nameof(NormalizeToUtcTooltip)] = "Нормалізувати всі часові мітки до UTC+0",
            [nameof(ParallelLimitLabel)] = "Паралельний ліміт",
            [nameof(ParallelLimitTooltip)] = "Скільки каналів може експортуватись одночасно",
            // Export Setup
            [nameof(ChannelsSelectedText)] = "каналів вибрано",
            [nameof(OutputPathLabel)] = "Шлях виведення",
            [nameof(FormatLabel)] = "Формат",
            [nameof(FormatTooltip)] = "Формат експорту",
            [nameof(AfterDateLabel)] = "Після (дата)",
            [nameof(AfterDateTooltip)] = "Включати лише повідомлення, надіслані після цієї дати",
            [nameof(BeforeDateLabel)] = "До (дата)",
            [nameof(BeforeDateTooltip)] = "Включати лише повідомлення, надіслані до цієї дати",
            [nameof(AfterTimeLabel)] = "Після (час)",
            [nameof(AfterTimeTooltip)] = "Включати лише повідомлення, надіслані після цього часу",
            [nameof(BeforeTimeLabel)] = "До (час)",
            [nameof(BeforeTimeTooltip)] = "Включати лише повідомлення, надіслані до цього часу",
            [nameof(PartitionLimitLabel)] = "Ліміт розподілу",
            [nameof(PartitionLimitTooltip)] =
                "Розділити вивід на частини, кожна обмежена вказаною кількістю повідомлень (напр. '100') або розміром файлу (напр. '10mb')",
            [nameof(MessageFilterLabel)] = "Фільтр повідомлень",
            [nameof(MessageFilterTooltip)] =
                "Включати лише повідомлення, що відповідають цьому фільтру (напр. 'from:foo#1234' або 'has:image'). Дивіться документацію для більш детальної інформації.",
            [nameof(FormatMarkdownLabel)] = "Форматувати markdown",
            [nameof(FormatMarkdownTooltip)] =
                "Обробляти markdown, згадки та інші спеціальні токени",
            [nameof(DownloadAssetsLabel)] = "Завантажувати ресурси",
            [nameof(DownloadAssetsTooltip)] =
                "Завантажувати ресурси, на які посилається експорт (аватари, вкладені файли, вбудовані зображення тощо)",
            [nameof(ReuseAssetsLabel)] = "Повторно використовувати ресурси",
            [nameof(ReuseAssetsTooltip)] =
                "Повторно використовувати раніше завантажені ресурси, щоб уникнути зайвих запитів",
            [nameof(AssetsDirPathLabel)] = "Шлях до директорії ресурсів",
            [nameof(AssetsDirPathTooltip)] =
                "Завантажувати ресурси до цієї директорії. Якщо не вказано, шлях до директорії ресурсів буде визначено з шляху виведення.",
            [nameof(AdvancedOptionsTooltip)] = "Перемкнути розширені параметри",
            [nameof(ExportButton)] = "ЕКСПОРТУВАТИ",
            // Common buttons
            [nameof(CloseButton)] = "ЗАКРИТИ",
            [nameof(CancelButton)] = "СКАСУВАТИ",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Дякуємо за підтримку України!",
            [nameof(UkraineSupportMessage)] = """
                Поки Росія веде геноцидну війну проти моєї країни, я вдячний кожному, хто продовжує підтримувати Україну у нашій боротьбі за свободу.

                Натисніть ДІЗНАТИСЬ БІЛЬШЕ, щоб знайти способи допомогти.
                """,
            [nameof(LearnMoreButton)] = "ДІЗНАТИСЬ БІЛЬШЕ",
            [nameof(UnstableBuildTitle)] = "Попередження про нестабільну збірку",
            [nameof(UnstableBuildMessage)] = """
                Ви використовуєте збірку розробки {0}. Ці збірки не пройшли ретельного тестування та можуть містити помилки.

                Авто-оновлення вимкнено для збірок розробки.

                Натисніть ПЕРЕГЛЯНУТИ РЕЛІЗИ, щоб завантажити стабільний реліз.
                """,
            [nameof(SeeReleasesButton)] = "ПЕРЕГЛЯНУТИ РЕЛІЗИ",
            [nameof(UpdateDownloadingMessage)] = "Завантаження оновлення {0} v{1}...",
            [nameof(UpdateReadyMessage)] = "Оновлення завантажено та буде встановлено після виходу",
            [nameof(UpdateInstallNowButton)] = "ВСТАНОВИТИ ЗАРАЗ",
            [nameof(UpdateFailedMessage)] = "Не вдалося виконати оновлення програми",
            [nameof(ErrorPullingServersTitle)] = "Помилка завантаження серверів",
            [nameof(ErrorPullingChannelsTitle)] = "Помилка завантаження каналів",
            [nameof(ErrorExportingTitle)] = "Помилка експорту каналу(-ів)",
            [nameof(SuccessfulExportMessage)] = "Успішно експортовано {0} канал(-ів)",
        };
}
