using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Cli.Tests.Infra;

public static class ChannelIds
{
    public static Snowflake AttachmentTestCases { get; } = Snowflake.Parse("885587741654536192");

    public static Snowflake DateRangeTestCases { get; } = Snowflake.Parse("866674248747319326");

    public static Snowflake EmbedTestCases { get; } = Snowflake.Parse("866472452459462687");

    public static Snowflake GroupingTestCases { get; } = Snowflake.Parse("992092091545034842");

    public static Snowflake FilterTestCases { get; } = Snowflake.Parse("866744075033641020");

    public static Snowflake MarkdownTestCases { get; } = Snowflake.Parse("866459526819348521");

    public static Snowflake MentionTestCases { get; } = Snowflake.Parse("866458801389174794");

    public static Snowflake ReplyTestCases { get; } = Snowflake.Parse("866459871934677052");

    public static Snowflake SelfContainedTestCases { get; } = Snowflake.Parse("887441432678379560");

    public static Snowflake StickerTestCases { get; } = Snowflake.Parse("939668868253769729");
}
