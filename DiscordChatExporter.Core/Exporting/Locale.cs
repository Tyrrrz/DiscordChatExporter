using System.Globalization;

namespace DiscordChatExporter.Core.Exporting;

// Basically a light-weight wrapper around CultureInfo
public readonly partial record struct Locale(string Code)
{
    // Method instead of property so as to not mess with the record's equality contract
    public CultureInfo ToCultureInfo() => CultureInfo.GetCultureInfo(Code);

    public override string ToString() => Code;
}

public partial record struct Locale
{
    public static Locale Current { get; } = FromCultureInfo(CultureInfo.CurrentCulture);

    public static Locale FromCultureInfo(CultureInfo cultureInfo) => new(cultureInfo.Name);
}
