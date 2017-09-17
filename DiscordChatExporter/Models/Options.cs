namespace DiscordChatExporter.Models
{
    public class Options
    {
        public string Token { get; }

        public string ChannelId { get; }

        public Theme Theme { get; }

        public Options(string token, string channelId, Theme theme)
        {
            Token = token;
            ChannelId = channelId;
            Theme = theme;
        }
    }
}