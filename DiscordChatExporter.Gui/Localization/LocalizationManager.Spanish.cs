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
            [nameof(TokenWatermark)] = "Token",
            // Token instructions (personal account)
            [nameof(TokenPersonalHeader)] = "Cómo obtener el token para tu cuenta personal:",
            [nameof(TokenPersonalTosWarning)] =
                "*  Automatizar cuentas de usuario técnicamente va en contra de los ToS — **bajo tu propio riesgo**!",
            [nameof(TokenPersonalInstructions)] = """
                1. Abre Discord en tu navegador web e inicia sesión
                2. Abre cualquier servidor o canal de mensaje directo
                3. Presiona **Ctrl+Shift+I** para mostrar las herramientas de desarrollo
                4. Navega a la pestaña **Network**
                5. Presiona **Ctrl+R** para recargar
                6. Cambia entre canales para activar solicitudes de red
                7. Busca una solicitud que comience con **messages**
                8. Selecciona la pestaña **Headers** a la derecha
                9. Desplázate hasta la sección **Request Headers**
                10. Copia el valor del encabezado **authorization**
                """,
            // Token instructions (bot)
            [nameof(TokenBotHeader)] = "Cómo obtener el token para tu bot:",
            [nameof(TokenBotInstructions)] = """
                El token se genera al crear el bot. Si lo perdiste, genera uno nuevo:

                1. Abre Discord [portal de desarrolladores](https://discord.com/developers/applications)
                2. Abre la configuración de tu aplicación
                3. Navega a la sección **Bot** en el lado izquierdo
                4. En **Token**, haz clic en **Reset Token**
                5. Haz clic en **Yes, do it!** y autentica para confirmar
                *  Las integraciones que usen el token anterior dejarán de funcionar hasta que se actualicen
                *  Tu bot necesita tener habilitado **Message Content Intent** para leer mensajes
                """,
            [nameof(TokenHelpText)] =
                "Si tienes preguntas o problemas, consulta la [documentación](https://github.com/Tyrrrz/DiscordChatExporter/tree/master/.docs)",
            // Settings
            [nameof(SettingsTitle)] = "Ajustes",
            [nameof(ThemeLabel)] = "Tema",
            [nameof(ThemeTooltip)] = "Tema de interfaz preferido",
            [nameof(LanguageLabel)] = "Idioma",
            [nameof(LanguageTooltip)] = "Idioma de interfaz preferido",
            [nameof(AutoUpdateLabel)] = "Actualización automática",
            [nameof(AutoUpdateTooltip)] = "Realizar actualizaciones automáticas en cada inicio",
            [nameof(PersistTokenLabel)] = "Guardar token",
            [nameof(PersistTokenTooltip)] = """
                Guardar el último token utilizado en un archivo para conservarlo entre sesiones.
                **Advertencia**: aunque el token se almacena con cifrado, aún puede ser recuperado por un atacante con acceso a tu sistema.
                """,
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
            [nameof(OutputPathTooltip)] = """
                Ruta del archivo o directorio de salida.

                Si se especifica un directorio, los nombres de archivo se generarán automáticamente según los nombres de los canales y los parámetros de exportación.

                Las rutas de directorio deben terminar con una barra diagonal para evitar ambigüedades.

                Tokens de plantilla disponibles:
                **%g** — ID del servidor
                **%G** — nombre del servidor
                **%t** — ID de categoría
                **%T** — nombre de categoría
                **%c** — ID del canal
                **%C** — nombre del canal
                **%p** — posición del canal
                **%P** — posición de la categoría
                **%a** — fecha desde
                **%b** — fecha hasta
                **%d** — fecha actual
                """,
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
            [nameof(ReverseMessageOrderLabel)] = "Invertir orden de mensajes",
            [nameof(ReverseMessageOrderTooltip)] =
                "Exportar mensajes en orden cronológico inverso (los más recientes primero)",
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
            [nameof(ErrorPullingGuildsTitle)] = "Error al cargar servidores",
            [nameof(ErrorPullingChannelsTitle)] = "Error al cargar canales",
            [nameof(ErrorExportingTitle)] = "Error al exportar canal(es)",
            [nameof(SuccessfulExportMessage)] = "{0} canal(es) exportado(s) con éxito",
        };
}
