using System.Collections.Generic;

namespace DiscordChatExporter.Gui.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> SpanishLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(PullGuildsTooltip)] = "Cargar servidores y canales disponibles (Enter)",
            [nameof(SettingsTooltip)] = "Ajustes",
            [nameof(LastMessageSentTooltip)] = "Último mensaje enviado:",
            // Settings
            [nameof(SettingsTitle)] = "Ajustes",
            [nameof(ThemeLabel)] = "Tema",
            [nameof(ThemeTooltip)] = "Tema de interfaz preferido",
            [nameof(LanguageLabel)] = "Idioma",
            [nameof(LanguageTooltip)] = "Idioma de interfaz preferido",
            [nameof(AutoUpdateLabel)] = "Actualización automática",
            [nameof(AutoUpdateTooltip)] = "Realizar actualizaciones automáticas en cada inicio",
            [nameof(PersistTokenLabel)] = "Guardar token",
            [nameof(PersistTokenTooltip)] =
                "Guardar el último token utilizado en un archivo para conservarlo entre sesiones",
            [nameof(RateLimitPreferenceLabel)] = "Preferencia de límite de velocidad",
            [nameof(RateLimitPreferenceTooltip)] =
                "Si se deben respetar los límites de velocidad recomendados. Si está desactivado, solo se respetarán los límites estrictos (respuestas 429).",
            [nameof(ShowThreadsLabel)] = "Mostrar hilos",
            [nameof(ShowThreadsTooltip)] = "Qué tipos de hilos mostrar en la lista de canales",
            [nameof(LocaleLabel)] = "Configuración regional",
            [nameof(LocaleTooltip)] = "Configuración regional para el formato de fechas y números",
            [nameof(NormalizeToUtcLabel)] = "Normalizar a UTC",
            [nameof(NormalizeToUtcTooltip)] = "Normalizar todas las marcas de tiempo a UTC+0",
            [nameof(ParallelLimitLabel)] = "Límite paralelo",
            [nameof(ParallelLimitTooltip)] = "Cuántos canales pueden exportarse al mismo tiempo",
            // Export Setup
            [nameof(ChannelsSelectedText)] = "canales seleccionados",
            [nameof(OutputPathLabel)] = "Ruta de salida",
            [nameof(FormatLabel)] = "Formato",
            [nameof(FormatTooltip)] = "Formato de exportación",
            [nameof(AfterDateLabel)] = "Después (fecha)",
            [nameof(AfterDateTooltip)] = "Solo incluir mensajes enviados después de esta fecha",
            [nameof(BeforeDateLabel)] = "Antes (fecha)",
            [nameof(BeforeDateTooltip)] = "Solo incluir mensajes enviados antes de esta fecha",
            [nameof(AfterTimeLabel)] = "Después (hora)",
            [nameof(AfterTimeTooltip)] = "Solo incluir mensajes enviados después de esta hora",
            [nameof(BeforeTimeLabel)] = "Antes (hora)",
            [nameof(BeforeTimeTooltip)] = "Solo incluir mensajes enviados antes de esta hora",
            [nameof(PartitionLimitLabel)] = "Límite de partición",
            [nameof(PartitionLimitTooltip)] =
                "Dividir la salida en particiones, cada una limitada al número de mensajes especificado (p. ej. '100') o tamaño de archivo (p. ej. '10mb')",
            [nameof(MessageFilterLabel)] = "Filtro de mensajes",
            [nameof(MessageFilterTooltip)] =
                "Solo incluir mensajes que satisfagan este filtro (p. ej. 'from:foo#1234' o 'has:image'). Consulte la documentación para más información.",
            [nameof(FormatMarkdownLabel)] = "Formatear markdown",
            [nameof(FormatMarkdownTooltip)] =
                "Procesar markdown, menciones y otros tokens especiales",
            [nameof(DownloadAssetsLabel)] = "Descargar recursos",
            [nameof(DownloadAssetsTooltip)] =
                "Descargar los recursos referenciados por la exportación (avatares, archivos adjuntos, imágenes incrustadas, etc.)",
            [nameof(ReuseAssetsLabel)] = "Reutilizar recursos",
            [nameof(ReuseAssetsTooltip)] =
                "Reutilizar recursos previamente descargados para evitar solicitudes redundantes",
            [nameof(AssetsDirPathLabel)] = "Ruta del directorio de recursos",
            [nameof(AssetsDirPathTooltip)] =
                "Descargar recursos en este directorio. Si no se especifica, la ruta se derivará de la ruta de salida.",
            [nameof(AdvancedOptionsTooltip)] = "Alternar opciones avanzadas",
            [nameof(ExportButton)] = "EXPORTAR",
            // Common buttons
            [nameof(CloseButton)] = "CERRAR",
            [nameof(CancelButton)] = "CANCELAR",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "¡Gracias por apoyar a Ucrania!",
            [nameof(UkraineSupportMessage)] = """
                Mientras Rusia libra una guerra genocida contra mi país, estoy agradecido con todos los que continúan apoyando a Ucrania en nuestra lucha por la libertad.

                Haga clic en MÁS INFORMACIÓN para encontrar formas de ayudar.
                """,
            [nameof(LearnMoreButton)] = "MÁS INFORMACIÓN",
            [nameof(UnstableBuildTitle)] = "Advertencia de versión inestable",
            [nameof(UnstableBuildMessage)] = """
                Está usando una versión de desarrollo de {0}. Estas versiones no han sido probadas exhaustivamente y pueden contener errores.

                Las actualizaciones automáticas están desactivadas para las versiones de desarrollo.

                Haga clic en VER VERSIONES si desea descargar una versión estable.
                """,
            [nameof(SeeReleasesButton)] = "VER VERSIONES",
            [nameof(UpdateDownloadingMessage)] = "Descargando actualización a {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "La actualización se ha descargado y se instalará al salir",
            [nameof(UpdateInstallNowButton)] = "INSTALAR AHORA",
            [nameof(UpdateFailedMessage)] = "Error al realizar la actualización de la aplicación",
            [nameof(ErrorPullingServersTitle)] = "Error al cargar servidores",
            [nameof(ErrorPullingChannelsTitle)] = "Error al cargar canales",
            [nameof(ErrorExportingTitle)] = "Error al exportar canal(es)",
            [nameof(SuccessfulExportMessage)] = "{0} canal(es) exportado(s) con éxito",
        };
}
