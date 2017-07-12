using CommandLine;
using CommandLine.Text;

namespace DiscordChatExporter
{
    public class Options
    {
        [Option('t', "token", Required = true, HelpText = "Discord access token")]
        public string Token { get; set; }

        [Option('c', "channel", Required = true, HelpText = "ID of the text channel to export")]
        public string ChannelId { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("DiscordChatExporter"),
                Copyright = new CopyrightInfo("Alexey 'Tyrrrz' Golub", 2017),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Usage: DiscordChatExporter.exe " +
                                   "-t REkOTVqm9RWOTNOLCdiuMpWd.QiglBz.Lub0E0TZ1xX4ZxCtnwtpBhWt3v1 " +
                                   "-c 459360869055190534");
            help.AddOptions(this);

            return help;
        }
    }
}