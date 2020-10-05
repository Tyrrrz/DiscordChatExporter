using MaterialDesignThemes.Wpf;
using System.Windows.Media;

namespace DiscordChatExporter.Gui
{
    public sealed class Theme
    {
        public static Theme Light { get; } = new Theme(new MaterialDesignLightTheme(), HexToColor.Convert("#343838"), HexToColor.Convert("#F9A825"));
        public static Theme Dark { get; } = new Theme(new MaterialDesignDarkTheme(), HexToColor.Convert("#E8E8E8"), HexToColor.Convert("#F9A825"));

        public static void SetCurrent(Theme theme)
        {
            var paletteHelper = new PaletteHelper();

            var materialTheme = paletteHelper.GetTheme();
            materialTheme.SetBaseTheme(theme.BaseTheme);
            materialTheme.SetPrimaryColor(theme.PrimaryColor);
            materialTheme.SetSecondaryColor(theme.SecondaryColor);

            paletteHelper.SetTheme(materialTheme);
        }

        public Theme(IBaseTheme baseTheme, Color primaryColor, Color secondaryColor)
        {
            BaseTheme = baseTheme;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
        }

        public IBaseTheme BaseTheme { get; }
        public Color PrimaryColor { get; }
        public Color SecondaryColor { get; }

        class HexToColor
        {
            public static Color Convert(string hex)
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
        }
    }
}
