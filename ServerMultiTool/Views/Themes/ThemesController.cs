using System;
using System.Windows;

namespace ServerMultiTool.Views.Themes
{
    public static class ThemesController
    {
        private static ThemeTypes _currentTheme;

        public enum ThemeTypes
        {
            Light, 
            Dark,
        }

        public static void ChangeTheme(ThemeTypes type)
        {
            if (_currentTheme == type)
                return;
            
            _currentTheme = type;
            
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary
            {
                Source = GetPathToResourceDictionary(type)
            };
        }

        private static Uri GetPathToResourceDictionary(ThemeTypes type)
        {
            var themeName = type switch
            {
                ThemeTypes.Dark => "DarkTheme",
                ThemeTypes.Light => "LightTheme",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown Theme Type: {type}")
            };

            return new Uri($"Views/Themes/{themeName}.xaml", UriKind.Relative);
        }
    }
}
