using System;
using System.Reflection;
using DiscordChatExporter.Gui.Utils;
using MaterialDesignThemes.Wpf;

namespace DiscordChatExporter.Gui;

public partial class App
{
    private static Assembly Assembly { get; } = typeof(App).Assembly;

    public static string Name { get; } = Assembly.GetName().Name!;

    public static Version Version { get; } = Assembly.GetName().Version!;

    public static string VersionString { get; } = Version.ToString(3);

    public static string ProjectUrl { get; } = "https://github.com/Tyrrrz/DiscordChatExporter";

    public static string ChangelogUrl { get; } = ProjectUrl + "/blob/master/Changelog.md";

    public static string DocumentationUrl { get; } = ProjectUrl + "/blob/master/.docs";
}

public partial class App
{
    private static Theme LightTheme { get; } =
        Theme.Create(
            new MaterialDesignLightTheme(),
            MediaColor.FromHex("#343838"),
            MediaColor.FromHex("#F9A825")
        );

    private static Theme DarkTheme { get; } =
        Theme.Create(
            new MaterialDesignDarkTheme(),
            MediaColor.FromHex("#E8E8E8"),
            MediaColor.FromHex("#F9A825")
        );

    public static void SetLightTheme()
    {
        var paletteHelper = new PaletteHelper();
        paletteHelper.SetTheme(LightTheme);
    }

    public static void SetDarkTheme()
    {
        var paletteHelper = new PaletteHelper();
        paletteHelper.SetTheme(DarkTheme);
    }
}
