using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Cli.Tests.TestData
{
    public static class ChannelIds
    {
        public static Snowflake EmbedTestCases { get; } = Snowflake.Parse("866472452459462687");

        public static Snowflake MentionTestCases { get; } = Snowflake.Parse("866458801389174794");

        public static Snowflake ReplyTestCases { get; } = Snowflake.Parse("866459871934677052");
    }
}