using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;

namespace AutomationShell48.UI.Features.Settings
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private readonly IThemeService _themeService;
        private readonly ILogger _logger;

        public SettingsPageViewModel(IThemeService themeService, ILogger logger)
        {
            _themeService = themeService;
            _logger = logger;
            Title = "Settings";
            Description = "App state is persisted using user-scoped Properties.Settings.";
            _logger?.Info("Settings view loaded.");
        }

        public string Description { get; }

        public string CurrentTheme => _themeService.CurrentTheme.ToString();
    }
}

