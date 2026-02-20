using System;
using System.Globalization;
using AutomationShell48.Core.Services;
using AutomationShell48.Core.Theming;

namespace AutomationShell48.Infrastructure.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly IUserSettingsProvider _provider;

        public AppSettingsService(IUserSettingsProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public ThemeKind LastTheme
        {
            get
            {
                var raw = _provider.Get(nameof(LastTheme), ThemeKind.Light.ToString());
                return Enum.TryParse(raw, out ThemeKind theme) ? theme : ThemeKind.Light;
            }
            set => _provider.Set(nameof(LastTheme), value.ToString());
        }

        public bool IsSidebarCollapsed
        {
            get => ParseBool(_provider.Get(nameof(IsSidebarCollapsed), "False"));
            set => _provider.Set(nameof(IsSidebarCollapsed), value.ToString(CultureInfo.InvariantCulture));
        }

        public bool IsRightSidebarCollapsed
        {
            get => ParseBool(_provider.Get(nameof(IsRightSidebarCollapsed), "False"));
            set => _provider.Set(nameof(IsRightSidebarCollapsed), value.ToString(CultureInfo.InvariantCulture));
        }

        public bool IsRightMenuEnabled
        {
            get => ParseBool(_provider.Get(nameof(IsRightMenuEnabled), "False"));
            set => _provider.Set(nameof(IsRightMenuEnabled), value.ToString(CultureInfo.InvariantCulture));
        }

        public string LastSelectedNavKey
        {
            get => _provider.Get(nameof(LastSelectedNavKey), "main");
            set => _provider.Set(nameof(LastSelectedNavKey), string.IsNullOrWhiteSpace(value) ? "main" : value);
        }

        public double WindowWidth
        {
            get => ParseDouble(_provider.Get(nameof(WindowWidth), "1280"));
            set => _provider.Set(nameof(WindowWidth), value.ToString(CultureInfo.InvariantCulture));
        }

        public double WindowHeight
        {
            get => ParseDouble(_provider.Get(nameof(WindowHeight), "800"));
            set => _provider.Set(nameof(WindowHeight), value.ToString(CultureInfo.InvariantCulture));
        }

        public double WindowTop
        {
            get => ParseDouble(_provider.Get(nameof(WindowTop), "100"));
            set => _provider.Set(nameof(WindowTop), value.ToString(CultureInfo.InvariantCulture));
        }

        public double WindowLeft
        {
            get => ParseDouble(_provider.Get(nameof(WindowLeft), "100"));
            set => _provider.Set(nameof(WindowLeft), value.ToString(CultureInfo.InvariantCulture));
        }

        public bool HasSavedWindowBounds
        {
            get => ParseBool(_provider.Get(nameof(HasSavedWindowBounds), "False"));
            set => _provider.Set(nameof(HasSavedWindowBounds), value.ToString(CultureInfo.InvariantCulture));
        }

        public void Save()
        {
            _provider.Save();
        }

        private static bool ParseBool(string value)
        {
            return bool.TryParse(value, out var result) && result;
        }

        private static double ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0d;
        }
    }
}
