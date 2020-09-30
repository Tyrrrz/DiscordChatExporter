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
        public static Theme Dark { get; } = new Theme(new MaterialDesignDarkTheme(), HexToColor.convert("#2196f3"), HexToColor.convert("#F9A825"));
        private Theme(IBaseTheme baseTheme, Color primaryColor, Color secondaryColor)
        {
            this.baseTheme = baseTheme;
            this.primaryColor = primaryColor;
            this.secondaryColor = secondaryColor;
        }

        public IBaseTheme baseTheme { get; }
        public Color primaryColor { get; }
        public Color secondaryColor { get; }

        class HexToColor
        {
            public static Color convert(string hex)
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
        }
    }
}
