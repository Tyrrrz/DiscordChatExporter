using CommandLine;

namespace DiscordChatExporter.Cli.Verbs.Options
{
    [Verb("exportguild", HelpText = "Export all available channels within a given guild.")]
    public class ExportGuildOptions : ExportOptions
    {
        [Option('g', "guild", Required = true, HelpText = "Guild ID.")]
        public string GuildId { get; set; }
    }
}