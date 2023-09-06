using System;
using System.Globalization;
using CliFx.Extensibility;
using DiscordChatExporter.Core.Exporting;

namespace DiscordChatExporter.Cli.Commands.Converters;

public class LocaleBindingConverter : BindingConverter<Locale>
{
    public override Locale Convert(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            throw new InvalidOperationException("Locale cannot be empty.");

        return Locale.FromCultureInfo(CultureInfo.GetCultureInfo(rawValue));
    }
}
