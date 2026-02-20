using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;
using System;

namespace AutomationShell48.UI.Features.Settings
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private readonly IThemeService _themeService;
        private readonly ILogger _logger;
        private readonly IAppSettingsService _settings;
        private readonly Action<bool> _onRightMenuEnabledChanged;
        private bool _isRightMenuEnabled;

        public SettingsPageViewModel(
            IThemeService themeService,
            ILogger logger,
            IAppSettingsService settings,
            Action<bool> onRightMenuEnabledChanged = null)
        {
            _themeService = themeService;
            _logger = logger;
            _settings = settings;
            _onRightMenuEnabledChanged = onRightMenuEnabledChanged;
            Title = "Settings";
            Description = "App state is persisted using user-scoped Properties.Settings.";
            IsRightMenuEnabled = _settings.IsRightMenuEnabled;
            _logger?.Info("Settings view loaded.");
        }

        public string Description { get; }

        public string CurrentTheme => _themeService.CurrentTheme.ToString();

        public bool IsRightMenuEnabled
        {
            get => _isRightMenuEnabled;
            set
            {
                if (!SetProperty(ref _isRightMenuEnabled, value))
                {
                    return;
                }

                _settings.IsRightMenuEnabled = value;
                _settings.Save();
                _onRightMenuEnabledChanged?.Invoke(value);
            }
        }
    }
}

