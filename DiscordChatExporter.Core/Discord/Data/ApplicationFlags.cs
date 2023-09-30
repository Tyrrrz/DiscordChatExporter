using System;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/application#application-object-application-flags
[Flags]
public enum ApplicationFlags
{
    None = 0,
    ApplicationAutoModerationRuleCreateBadge = 64,
    GatewayPresence = 4096,
    GatewayPresenceLimited = 8192,
    GatewayGuildMembers = 16384,
    GatewayGuildMembersLimited = 32768,
    VerificationPendingGuildLimit = 65536,
    Embedded = 131072,
    GatewayMessageContent = 262144,
    GatewayMessageContentLimited = 524288,
    ApplicationCommandBadge = 8388608
}
