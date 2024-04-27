using System.Globalization;

namespace DiscordChatExporter.Gui.Utils;

internal static class Internationalization
{
    public static bool Is24Hours =>
        string.IsNullOrWhiteSpace(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator)
        && string.IsNullOrWhiteSpace(CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator);

    public static string AvaloniaClockIdentifier => Is24Hours ? "24HourClock" : "12HourClock";
}
