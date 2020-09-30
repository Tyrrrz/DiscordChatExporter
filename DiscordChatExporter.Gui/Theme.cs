using MaterialDesignColors;
using MaterialDesignColors.Recommended;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace DiscordChatExporter.Gui
{
    public sealed class Theme
    {
        public static Theme Light { get; } = new Theme(new MaterialDesignLightTheme(), HexToColor.convert("#343838"), HexToColor.convert("#F9A825"));
        public static Theme Dark { get; } = new Theme(new MaterialDesignDarkTheme(), HexToColor.convert("#cbc7c7"), HexToColor.convert("#F9A825"));

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
            public static Color convert(string hex)
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
        }
    }
}
