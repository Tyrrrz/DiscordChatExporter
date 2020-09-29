using System;
using System.Reflection;
using MaterialDesignThemes.Wpf;

namespace DiscordChatExporter.Gui
{
    public partial class App
    {
        private static readonly Assembly Assembly = typeof(App).Assembly;

        public static string Name => Assembly.GetName().Name!;

        public static Version Version => Assembly.GetName().Version!;

        public static string VersionString => Version.ToString(3);
        public void setBaseTheme(IBaseTheme baseTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(baseTheme);
            paletteHelper.SetTheme(theme);
        }
    }
}