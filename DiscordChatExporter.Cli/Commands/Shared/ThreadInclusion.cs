using System;

namespace DiscordChatExporter.Cli.Commands.Shared;

[Flags]
public enum ThreadInclusion
{
    None = 0,
    Active = 1,
    Archived = 2,
    All = Active | Archived
}
