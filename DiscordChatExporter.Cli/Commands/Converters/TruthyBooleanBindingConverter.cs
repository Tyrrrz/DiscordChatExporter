using System.Globalization;
using CliFx.Extensibility;

namespace DiscordChatExporter.Cli.Commands.Converters;

internal class TruthyBooleanBindingConverter : BindingConverter<bool>
{
    public override bool Convert(string? rawValue)
    {
        // Null is still considered true, to match the base behavior
        if (rawValue is null)
            return true;

        if (string.IsNullOrWhiteSpace(rawValue))
            return false;

        if (bool.TryParse(rawValue, out var boolValue))
            return boolValue;

        if (int.TryParse(rawValue, CultureInfo.InvariantCulture, out var intValue) && intValue == 0)
            return false;

        return true;
    }
}
