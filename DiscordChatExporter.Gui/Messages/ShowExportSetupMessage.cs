using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Gui.Messages
{
    public class ShowExportSetupMessage
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public ShowExportSetupMessage(Guild guild, Channel channel)
        {
            Guild = guild;
            Channel = channel;
        }
    }
}