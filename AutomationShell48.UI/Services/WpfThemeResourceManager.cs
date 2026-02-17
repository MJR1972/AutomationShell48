using System;
using System.Linq;
using System.Windows;
using AutomationShell48.Core.Services;
using AutomationShell48.Core.Theming;

namespace AutomationShell48.UI.Services
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
            var oldTheme = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                d.Source.OriginalString.IndexOf("/Themes/", StringComparison.OrdinalIgnoreCase) >= 0);

            if (oldTheme != null)
            {
                dictionaries.Remove(oldTheme);
            }

            var uri = kind == ThemeKind.Dark
                ? new Uri("Themes/Dark.xaml", UriKind.Relative)
                : new Uri("Themes/Light.xaml", UriKind.Relative);

            dictionaries.Add(new ResourceDictionary { Source = uri });
        }
    }
}
