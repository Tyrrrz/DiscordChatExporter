using System;
using CliFx.Extensibility;
using DiscordChatExporter.Cli.Commands.Shared;

namespace DiscordChatExporter.Cli.Commands.Converters;

internal class ThreadInclusionBindingConverter : BindingConverter<ThreadInclusion>
{
    public override ThreadInclusion Convert(string? rawValue)
    {
        // Empty or unset value is treated as 'active' to match the previous behavior
        if (string.IsNullOrWhiteSpace(rawValue))
            return ThreadInclusion.Active;

        // Boolean 'true' is treated as 'active', boolean 'false' is treated as 'none'
        if (bool.TryParse(rawValue, out var boolValue))
            return boolValue ? ThreadInclusion.Active : ThreadInclusion.None;

        // Otherwise, fall back to regular enum parsing
        return Enum.Parse<ThreadInclusion>(rawValue, true);
    }
}
