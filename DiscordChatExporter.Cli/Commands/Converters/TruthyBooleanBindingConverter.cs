using System.Globalization;
using CliFx.Extensibility;

namespace DiscordChatExporter.Cli.Commands.Converters;

internal class TruthyBooleanBindingConverter : BindingConverter<bool>
{
    public override bool Convert(string? rawValue)
    {
        // Empty or unset value is treated as 'true', to match the regular boolean behavior
        if (string.IsNullOrWhiteSpace(rawValue))
            return true;

        // Number '1' is treated as 'true', other numbers are treated as 'false'
        if (int.TryParse(rawValue, CultureInfo.InvariantCulture, out var intValue))
            return intValue == 1;

        // Otherwise, fall back to regular boolean parsing
        return bool.Parse(rawValue);
    }
}
