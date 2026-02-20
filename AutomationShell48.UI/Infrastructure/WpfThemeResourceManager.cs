using System;
using System.Linq;
using System.Windows;
using AutomationShell48.Core.Services;
using AutomationShell48.Core.Theming;

namespace AutomationShell48.UI.Infrastructure
{
    public class WpfThemeResourceManager : IThemeResourceManager
    {
        public void ApplyThemeResources(ThemeKind kind)
        {
            var app = Application.Current;
            if (app == null)
            {
                return;
            }

            var dictionaries = app.Resources.MergedDictionaries;
            var oldThemes = dictionaries
                .Where(d => IsThemeDictionary(d.Source))
                .ToList();
            foreach (var oldTheme in oldThemes)
            {
                dictionaries.Remove(oldTheme);
            }

            var uri = kind == ThemeKind.Dark
                ? new Uri("Shared/Resources/Dark.xaml", UriKind.Relative)
                : new Uri("Shared/Resources/Light.xaml", UriKind.Relative);

            dictionaries.Add(new ResourceDictionary { Source = uri });
        }

        private static bool IsThemeDictionary(Uri source)
        {
            if (source == null)
            {
                return false;
            }

            var path = source.OriginalString.Replace('\\', '/');
            return path.EndsWith("/Shared/Resources/Light.xaml", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith("/Shared/Resources/Dark.xaml", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith("Shared/Resources/Light.xaml", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith("Shared/Resources/Dark.xaml", StringComparison.OrdinalIgnoreCase);
        }
    }
}

