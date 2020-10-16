using System;
using System.Reflection;
using DiscordChatExporter.Gui.Internal;
using MaterialDesignThemes.Wpf;

namespace DiscordChatExporter.Gui
{
    public partial class App
    {
        private static readonly Assembly Assembly = typeof(App).Assembly;

        public static string Name => Assembly.GetName().Name!;

        public static Version Version => Assembly.GetName().Version!;

        public static string VersionString => Version.ToString(3);
    }

    public partial class App
    {
        private static Theme LightTheme { get; } = Theme.Create(
            new MaterialDesignLightTheme(),
            MediaColor.FromHex("#343838"),
            MediaColor.FromHex("#F9A825")
        );

        private static Theme DarkTheme { get; } = Theme.Create(
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
}