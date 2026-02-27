using System.Collections.Generic;

namespace DiscordChatExporter.Gui.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> FrenchLocalization = new Dictionary<
        string,
        string
    >
    {
        // Dashboard
        [nameof(PullGuildsTooltip)] = "Charger les serveurs et canaux disponibles (Entrée)",
        [nameof(SettingsTooltip)] = "Paramètres",
        [nameof(LastMessageSentTooltip)] = "Dernier message envoyé :",
        [nameof(TokenWatermark)] = "Token",
        // Token instructions (personal account)
        [nameof(TokenPersonalHeader)] = "Obtenir le token pour votre compte personnel :",
        [nameof(TokenPersonalTosWarning)] =
            "*  L'automatisation des comptes est techniquement contraire aux CGU — **à vos risques et périls**!",
        [nameof(TokenPersonalInstructions)] = """
            1. Ouvrez Discord dans votre navigateur web et connectez-vous
            2. Ouvrez n'importe quel serveur ou canal de message direct
            3. Appuyez sur **Ctrl+Shift+I** pour afficher les outils de développement
            4. Naviguez vers l'onglet **Network**
            5. Appuyez sur **Ctrl+R** pour recharger
            6. Changez de canal pour déclencher des requêtes réseau
            7. Cherchez une requête commençant par **messages**
            8. Sélectionnez l'onglet **Headers** à droite
            9. Faites défiler jusqu'à la section **Request Headers**
            10. Copiez la valeur de l'en-tête **authorization**
            """,
        // Token instructions (bot)
        [nameof(TokenBotHeader)] = "Obtenir le token pour votre bot :",
        [nameof(TokenBotInstructions)] = """
            Le token est généré lors de la création du bot. Si vous l'avez perdu, générez-en un nouveau :

            1. Ouvrez Discord [portail développeur](https://discord.com/developers/applications)
            2. Ouvrez les paramètres de votre application
            3. Naviguez vers la section **Bot** à gauche
            4. Sous **Token**, cliquez sur **Reset Token**
            5. Cliquez sur **Yes, do it!** et confirmez
            *  Les intégrations utilisant l'ancien token cesseront de fonctionner jusqu'à leur mise à jour
            *  Votre bot doit avoir l'option **Message Content Intent** activée pour lire les messages
            """,
        [nameof(TokenHelpText)] =
            "Pour les questions ou problèmes, veuillez consulter la [documentation](https://github.com/Tyrrrz/DiscordChatExporter/tree/master/.docs)",
        // Settings
        [nameof(SettingsTitle)] = "Paramètres",
        [nameof(ThemeLabel)] = "Thème",
        [nameof(ThemeTooltip)] = "Thème d'interface préféré",
        [nameof(LanguageLabel)] = "Langue",
        [nameof(LanguageTooltip)] = "Langue d'interface préférée",
        [nameof(AutoUpdateLabel)] = "Mise à jour automatique",
        [nameof(AutoUpdateTooltip)] = "Effectuer des mises à jour automatiques à chaque lancement",
        [nameof(PersistTokenLabel)] = "Conserver le token",
        [nameof(PersistTokenTooltip)] =
            """
            Enregistrer le dernier token utilisé dans un fichier pour le conserver entre les sessions.
            **Avertissement** : bien que le token soit stocké chiffré, il peut toujours être récupéré par un attaquant ayant accès à votre système.
            """,
        [nameof(RateLimitPreferenceLabel)] = "Préférence de limite de débit",
        [nameof(RateLimitPreferenceTooltip)] =
            "Indique s'il faut respecter les limites de débit recommandées. Si désactivé, seules les limites strictes (réponses 429) seront respectées.",
        [nameof(ShowThreadsLabel)] = "Afficher les fils",
        [nameof(ShowThreadsTooltip)] = "Quels types de fils afficher dans la liste des canaux",
        [nameof(LocaleLabel)] = "Locale",
        [nameof(LocaleTooltip)] = "Locale à utiliser pour le formatage des dates et des nombres",
        [nameof(NormalizeToUtcLabel)] = "Normaliser en UTC",
        [nameof(NormalizeToUtcTooltip)] = "Normaliser tous les horodatages en UTC+0",
        [nameof(ParallelLimitLabel)] = "Limite parallèle",
        [nameof(ParallelLimitTooltip)] = "Combien de canaux peuvent être exportés simultanément",
        // Export Setup
        [nameof(ChannelsSelectedText)] = "canaux sélectionnés",
        [nameof(OutputPathLabel)] = "Chemin de sortie",
        [nameof(OutputPathTooltip)] = """
            Chemin du fichier ou répertoire de sortie.

            Si un répertoire est spécifié, les noms de fichiers seront générés automatiquement en fonction des noms de canaux et des paramètres d'exportation.

            Les chemins de répertoire doivent se terminer par un slash pour éviter toute ambiguïté.

            Jetons de modèle disponibles :
            - **%g** — ID du serveur
            - **%G** — nom du serveur
            - **%t** — ID de la catégorie
            - **%T** — nom de la catégorie
            - **%c** — ID du canal
            - **%C** — nom du canal
            - **%p** — position du canal
            - **%P** — position de la catégorie
            - **%a** — date après
            - **%b** — date avant
            - **%d** — date actuelle
            """,
        [nameof(FormatLabel)] = "Format",
        [nameof(FormatTooltip)] = "Format d'exportation",
        [nameof(AfterDateLabel)] = "Après (date)",
        [nameof(AfterDateTooltip)] = "Inclure uniquement les messages envoyés après cette date",
        [nameof(BeforeDateLabel)] = "Avant (date)",
        [nameof(BeforeDateTooltip)] = "Inclure uniquement les messages envoyés avant cette date",
        [nameof(AfterTimeLabel)] = "Après (heure)",
        [nameof(AfterTimeTooltip)] = "Inclure uniquement les messages envoyés après cette heure",
        [nameof(BeforeTimeLabel)] = "Avant (heure)",
        [nameof(BeforeTimeTooltip)] = "Inclure uniquement les messages envoyés avant cette heure",
        [nameof(PartitionLimitLabel)] = "Limite de partition",
        [nameof(PartitionLimitTooltip)] =
            "Diviser la sortie en partitions, chacune limitée au nombre de messages spécifié (ex. '100') ou à la taille de fichier (ex. '10mb')",
        [nameof(MessageFilterLabel)] = "Filtre de messages",
        [nameof(MessageFilterTooltip)] =
            "Inclure uniquement les messages satisfaisant ce filtre (ex. 'from:foo#1234' ou 'has:image'). Voir la documentation pour plus d'informations.",
        [nameof(ReverseMessageOrderLabel)] = "Inverser l'ordre des messages",
        [nameof(ReverseMessageOrderTooltip)] =
            "Exporter les messages en ordre chronologique inversé (les plus récents en premier)",
        [nameof(FormatMarkdownLabel)] = "Formater le markdown",
        [nameof(FormatMarkdownTooltip)] =
            "Traiter le markdown, les mentions et autres tokens spéciaux",
        [nameof(DownloadAssetsLabel)] = "Télécharger les ressources",
        [nameof(DownloadAssetsTooltip)] =
            "Télécharger les ressources référencées par l'export (avatars, fichiers joints, images intégrées, etc.)",
        [nameof(ReuseAssetsLabel)] = "Réutiliser les ressources",
        [nameof(ReuseAssetsTooltip)] =
            "Réutiliser les ressources précédemment téléchargées pour éviter les requêtes redondantes",
        [nameof(AssetsDirPathLabel)] = "Chemin du dossier des ressources",
        [nameof(AssetsDirPathTooltip)] =
            "Télécharger les ressources dans ce dossier. Si non spécifié, le chemin sera dérivé du chemin de sortie.",
        [nameof(AdvancedOptionsTooltip)] = "Basculer les options avancées",
        [nameof(ExportButton)] = "EXPORTER",
        // Common buttons
        [nameof(CloseButton)] = "FERMER",
        [nameof(CancelButton)] = "ANNULER",
        // Dialog messages
        [nameof(UkraineSupportTitle)] = "Merci de soutenir l'Ukraine !",
        [nameof(UkraineSupportMessage)] = """
            Alors que la Russie mène une guerre génocidaire contre mon pays, je suis reconnaissant envers tous ceux qui continuent à soutenir l'Ukraine dans notre lutte pour la liberté.

            Cliquez sur EN SAVOIR PLUS pour trouver des moyens d'aider.
            """,
        [nameof(LearnMoreButton)] = "EN SAVOIR PLUS",
        [nameof(UnstableBuildTitle)] = "Avertissement : version instable",
        [nameof(UnstableBuildMessage)] = """
            Vous utilisez une version de développement de {0}. Ces versions ne sont pas rigoureusement testées et peuvent contenir des bugs.

            Les mises à jour automatiques sont désactivées pour les versions de développement.

            Cliquez sur VOIR LES VERSIONS pour télécharger une version stable.
            """,
        [nameof(SeeReleasesButton)] = "VOIR LES VERSIONS",
        [nameof(UpdateDownloadingMessage)] = "Téléchargement de la mise à jour vers {0} v{1}...",
        [nameof(UpdateReadyMessage)] =
            "La mise à jour a été téléchargée et sera installée à la fermeture",
        [nameof(UpdateInstallNowButton)] = "INSTALLER MAINTENANT",
        [nameof(UpdateFailedMessage)] = "Échec de la mise à jour de l'application",
        [nameof(ErrorPullingGuildsTitle)] = "Erreur lors du chargement des serveurs",
        [nameof(ErrorPullingChannelsTitle)] = "Erreur lors du chargement des canaux",
        [nameof(ErrorExportingTitle)] = "Erreur lors de l'exportation des canaux",
        [nameof(SuccessfulExportMessage)] = "{0} canal(-aux) exporté(s) avec succès",
    };
}
