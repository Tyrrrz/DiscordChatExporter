using System;
using System.Reflection;
using System.Windows.Media;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace DiscordChatExporter.Gui
{
    public partial class App
    {
        private static readonly Assembly Assembly = typeof(App).Assembly;

        public static string Name => Assembly.GetName().Name!;

        public static Version Version => Assembly.GetName().Version!;

        public static string VersionString => Version.ToString(3);
        public void SetTheme(Theme theme)
        {
            var paletteHelper = new PaletteHelper();
            var mdTheme = paletteHelper.GetTheme();
            mdTheme.SetBaseTheme(theme.BaseTheme);
            mdTheme.SetPrimaryColor(theme.PrimaryColor);
            mdTheme.SetSecondaryColor(theme.SecondaryColor);

            paletteHelper.SetTheme(mdTheme);
        }
    }
}