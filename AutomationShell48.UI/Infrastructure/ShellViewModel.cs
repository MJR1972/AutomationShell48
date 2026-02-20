using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AutomationShell48.Core.Dialogs;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Navigation;
using AutomationShell48.Core.Services;
using AutomationShell48.Core.Theming;
using AutomationShell48.UI.Features.About;
using AutomationShell48.UI.Features.Dashboard;
using AutomationShell48.UI.Features.Projects;
using AutomationShell48.UI.Features.Settings;
using AutomationShell48.UI.Tools.LogViewer;

namespace AutomationShell48.UI.Infrastructure
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly ILogger _logger;
        private readonly IFileLogWriter _fileLogWriter;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IThemeService _themeService;
        private readonly IToolWindowService _toolWindowService;
        private readonly IAppSettingsService _settings;
        private readonly Dictionary<string, Func<BaseViewModel>> _viewFactories;

        private BaseViewModel _currentViewModel;
        private string _statusText;
        private string _currentTime;
        private bool _isSidebarCollapsed;
        private bool _isRightSidebarCollapsed;
        private bool _isRightMenuEnabled;
        private GridLength _rightSidebarWidth;
        private bool _isDialogOpen;
        private bool _isSettingsActive;
        private DialogViewModelBase _dialogViewModel;
        private string _currentPageTitle;

        public ShellViewModel(
            ILogger logger,
            IFileLogWriter fileLogWriter,
            INavigationService navigationService,
            IDialogService dialogService,
            IThemeService themeService,
            IToolWindowService toolWindowService,
            IAppSettingsService settings)
        {
            _logger = logger;
            _fileLogWriter = fileLogWriter;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _themeService = themeService;
            _toolWindowService = toolWindowService;
            _settings = settings;

            NavigationGroups = new ObservableCollection<NavigationGroup>();
            BuildNavigation();
            RightNavigationGroups = new ObservableCollection<NavigationGroup>();
            BuildRightNavigation();

            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            ToggleRightSidebarCommand = new RelayCommand(ToggleRightSidebar);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            OpenLogViewerCommand = new RelayCommand(OpenLogViewer);
            NavigateCommand = new RelayCommand<NavigationItem>(item => _navigationService.NavigateTo(item?.Key));
            RightNavigateCommand = new RelayCommand<NavigationItem>(OnRightNavigate);
            CloseDialogCommand = new RelayCommand(() => _dialogService.CloseDialog(false), () => IsDialogOpen);

            _viewFactories = new Dictionary<string, Func<BaseViewModel>>(StringComparer.OrdinalIgnoreCase)
            {
                ["main"] = () => new DashboardViewModel(_logger),
                ["projects"] = () => new ProjectsViewModel(_dialogService, _logger),
                ["settings"] = () => new SettingsPageViewModel(_themeService, _logger, _settings, enabled => IsRightMenuEnabled = enabled),
                ["about"] = () => new AboutViewModel()
            };

            _navigationService.Navigated += OnNavigated;
            _dialogService.DialogOpened += OnDialogOpened;
            _dialogService.DialogClosed += OnDialogClosed;

            IsSidebarCollapsed = _settings.IsSidebarCollapsed;
            IsRightSidebarCollapsed = _settings.IsRightSidebarCollapsed;
            IsRightMenuEnabled = _settings.IsRightMenuEnabled;
            UpdateRightSidebarWidth();
            UpdateThemeFields();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            StatusText = "Ready";
        }

        public ObservableCollection<NavigationGroup> NavigationGroups { get; }
        public ObservableCollection<NavigationGroup> RightNavigationGroups { get; }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public DialogViewModelBase DialogViewModel
        {
            get => _dialogViewModel;
            set => SetProperty(ref _dialogViewModel, value);
        }

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set
            {
                if (SetProperty(ref _isDialogOpen, value))
                {
                    CloseDialogCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsSettingsActive
        {
            get => _isSettingsActive;
            private set => SetProperty(ref _isSettingsActive, value);
        }

        public bool IsRightMenuEnabled
        {
            get => _isRightMenuEnabled;
            set
            {
                if (SetProperty(ref _isRightMenuEnabled, value))
                {
                    _settings.IsRightMenuEnabled = value;
                    _settings.Save();
                    UpdateRightSidebarWidth();
                }
            }
        }

        public GridLength RightSidebarWidth
        {
            get => _rightSidebarWidth;
            private set => SetProperty(ref _rightSidebarWidth, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set
            {
                if (SetProperty(ref _isSidebarCollapsed, value))
                {
                    _settings.IsSidebarCollapsed = value;
                    _settings.Save();
                }
            }
        }

        public bool IsRightSidebarCollapsed
        {
            get => _isRightSidebarCollapsed;
            set
            {
                if (SetProperty(ref _isRightSidebarCollapsed, value))
                {
                    _settings.IsRightSidebarCollapsed = value;
                    _settings.Save();
                    UpdateRightSidebarWidth();
                }
            }
        }

        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set => SetProperty(ref _currentPageTitle, value);
        }

        public string CurrentThemeLabel => "Theme: " + _themeService.CurrentTheme;

        public string ThemeIconKey => _themeService.CurrentTheme == ThemeKind.Dark ? "IconSun" : "IconMoon";

        public string LogIconKey => "IconLogs";

        public RelayCommand ToggleSidebarCommand { get; }

        public RelayCommand ToggleThemeCommand { get; }

        public RelayCommand OpenLogViewerCommand { get; }

        public RelayCommand<NavigationItem> NavigateCommand { get; }
        public RelayCommand<NavigationItem> RightNavigateCommand { get; }
        public RelayCommand ToggleRightSidebarCommand { get; }

        public RelayCommand CloseDialogCommand { get; }

        private void BuildNavigation()
        {
            var general = new NavigationGroup("General");
            general.Items.Add(new NavigationItem("main", "Main", "IconHome"));
            general.Items.Add(new NavigationItem("projects", "Projects", "IconFolder"));

            var system = new NavigationGroup("System");
            system.Items.Add(new NavigationItem("settings", "Settings", "IconSettings"));
            system.Items.Add(new NavigationItem("about", "About", "IconInfo"));

            NavigationGroups.Add(general);
            NavigationGroups.Add(system);
        }

        private void OnNavigated(string key)
        {
            if (!_viewFactories.TryGetValue(key, out var factory))
            {
                return;
            }

            CurrentViewModel = factory();
            IsSettingsActive = CurrentViewModel is SettingsPageViewModel;
            CurrentPageTitle = CurrentViewModel.Title;
            StatusText = "Navigated to " + CurrentViewModel.Title;

            foreach (var group in NavigationGroups)
            {
                foreach (var item in group.Items)
                {
                    item.IsSelected = string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase);
                }
            }
            UpdateRightSelection(key);

            _settings.LastSelectedNavKey = key;
            _settings.Save();
            _logger?.Info("Navigation: " + key);
        }

        private void OnDialogOpened(DialogViewModelBase viewModel)
        {
            DialogViewModel = viewModel;
            IsDialogOpen = true;
        }

        private void OnDialogClosed()
        {
            IsDialogOpen = false;
            DialogViewModel = null;
        }

        private void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
        }

        private void ToggleRightSidebar()
        {
            IsRightSidebarCollapsed = !IsRightSidebarCollapsed;
        }

        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
            _settings.LastTheme = _themeService.CurrentTheme;
            _settings.Save();
            UpdateThemeFields();
            StatusText = "Theme changed to " + _themeService.CurrentTheme;
        }

        private void OpenLogViewer()
        {
            var vm = new LogViewerViewModel(_fileLogWriter);
            _toolWindowService.ShowTool(vm);
        }

        private void BuildRightNavigation()
        {
            var tools = new NavigationGroup("Tools");
            tools.Items.Add(new NavigationItem("logs", "Logs", "IconLogs"));
            tools.Items.Add(new NavigationItem("diagnostics", "Diagnostics", "IconSettings"));

            var shortcuts = new NavigationGroup("Shortcuts");
            shortcuts.Items.Add(new NavigationItem("settings", "Settings", "IconSettings"));
            shortcuts.Items.Add(new NavigationItem("about", "About", "IconInfo"));

            var help = new NavigationGroup("Help");
            help.Items.Add(new NavigationItem("docs", "Documentation", "IconInfo"));
            help.Items.Add(new NavigationItem("support", "Support", "IconInfo"));

            RightNavigationGroups.Add(tools);
            RightNavigationGroups.Add(shortcuts);
            RightNavigationGroups.Add(help);
        }

        private void OnRightNavigate(NavigationItem item)
        {
            if (item == null)
            {
                return;
            }

            switch (item.Key)
            {
                case "logs":
                    OpenLogViewer();
                    break;
                case "settings":
                case "about":
                    _navigationService.NavigateTo(item.Key);
                    break;
                case "diagnostics":
                    _ = _dialogService.ShowDialogAsync(new MessageDialogViewModel("Diagnostics", "Diagnostics panel is not yet implemented."));
                    break;
                case "docs":
                    _ = _dialogService.ShowDialogAsync(new MessageDialogViewModel("Documentation", "Documentation shortcut is not yet configured."));
                    break;
                case "support":
                    _ = _dialogService.ShowDialogAsync(new MessageDialogViewModel("Support", "Support shortcut is not yet configured."));
                    break;
                default:
                    _ = _dialogService.ShowDialogAsync(new MessageDialogViewModel("Action", item.DisplayName + " is not yet implemented."));
                    break;
            }

            UpdateRightSelection(item.Key);
        }

        private void UpdateRightSelection(string key)
        {
            foreach (var group in RightNavigationGroups)
            {
                foreach (var item in group.Items)
                {
                    item.IsSelected = string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private void UpdateThemeFields()
        {
            OnPropertyChanged(nameof(CurrentThemeLabel));
            OnPropertyChanged(nameof(ThemeIconKey));
        }

        private void UpdateRightSidebarWidth()
        {
            if (!IsRightMenuEnabled)
            {
                RightSidebarWidth = new GridLength(0);
                return;
            }

            RightSidebarWidth = IsRightSidebarCollapsed
                ? new GridLength(72)
                : new GridLength(250);
        }
    }
}

