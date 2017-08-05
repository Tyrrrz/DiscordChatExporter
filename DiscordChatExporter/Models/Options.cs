namespace DiscordChatExporter.Models
{
    public class Options
    {
        public string Token { get; }

        public string ChannelId { get; }

        public Options(string token, string channelId)
        {
            Token = token;
            ChannelId = channelId;
        }
    }
}