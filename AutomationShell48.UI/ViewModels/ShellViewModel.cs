using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Navigation;
using AutomationShell48.Core.Services;
using AutomationShell48.Core.Theming;
using AutomationShell48.UI.ViewModels.Tools;

namespace AutomationShell48.UI.ViewModels
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
        private bool _isDialogOpen;
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

            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            OpenLogViewerCommand = new RelayCommand(OpenLogViewer);
            NavigateCommand = new RelayCommand<NavigationItem>(item => _navigationService.NavigateTo(item?.Key));
            CloseDialogCommand = new RelayCommand(() => _dialogService.CloseDialog(false), () => IsDialogOpen);

            _viewFactories = new Dictionary<string, Func<BaseViewModel>>(StringComparer.OrdinalIgnoreCase)
            {
                ["main"] = () => new DashboardViewModel(_logger),
                ["projects"] = () => new ProjectsViewModel(_dialogService, _logger),
                ["settings"] = () => new SettingsPageViewModel(_themeService, _logger),
                ["about"] = () => new AboutViewModel()
            };

            _navigationService.Navigated += OnNavigated;
            _dialogService.DialogOpened += OnDialogOpened;
            _dialogService.DialogClosed += OnDialogClosed;

            IsSidebarCollapsed = _settings.IsSidebarCollapsed;
            UpdateThemeFields();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            StatusText = "Ready";
        }

        public ObservableCollection<NavigationGroup> NavigationGroups { get; }

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
            CurrentPageTitle = CurrentViewModel.Title;
            StatusText = "Navigated to " + CurrentViewModel.Title;

            foreach (var group in NavigationGroups)
            {
                foreach (var item in group.Items)
                {
                    item.IsSelected = string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase);
                }
            }

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

        private void UpdateThemeFields()
        {
            OnPropertyChanged(nameof(CurrentThemeLabel));
            OnPropertyChanged(nameof(ThemeIconKey));
        }
    }
}
