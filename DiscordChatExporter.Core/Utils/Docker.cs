using System;

namespace DiscordChatExporter.Core.Utils;

public static class Docker
{
    public static bool IsRunningInDocker { get; } =
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
}
