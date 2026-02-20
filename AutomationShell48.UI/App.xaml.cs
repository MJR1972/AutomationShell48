using System;
using System.Windows;
using System.Windows.Threading;
using AutomationShell48.Core.Services;
using AutomationShell48.Infrastructure.Services;
using AutomationShell48.UI.Infrastructure;

using AutomationShell48.UI.Tools.LogViewer;


namespace AutomationShell48.UI
{
    public partial class App : Application
    {
        private IAppSettingsService _settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            var settingsProvider = new UserSettingsProvider();
            _settings = new AppSettingsService(settingsProvider);

            var fileWriter = new FileLogWriter();
            var eventWriter = new EventLogWriter(fileWriter);
            var logger = new Logger(fileWriter, eventWriter);

            var themeResourceManager = new WpfThemeResourceManager();
            var themeService = new ThemeService(themeResourceManager);

            var dialogService = new DialogService(logger);
            var navigationService = new NavigationService();
            var toolWindowService = new ToolWindowService(() => Current?.MainWindow);
            var exceptionHandler = new ExceptionHandler(logger, dialogService);

            AppServices.RegisterSingleton<IFileLogWriter>(fileWriter);
            AppServices.RegisterSingleton<IEventLogWriter>(eventWriter);
            AppServices.RegisterSingleton<ILogger>(logger);
            AppServices.RegisterSingleton<IThemeService>(themeService);
            AppServices.RegisterSingleton<IDialogService>(dialogService);
            AppServices.RegisterSingleton<INavigationService>(navigationService);
            AppServices.RegisterSingleton<IToolWindowService>(toolWindowService);
            AppServices.RegisterSingleton<IAppSettingsService>(_settings);
            AppServices.RegisterSingleton<IExceptionHandler>(exceptionHandler);

            themeService.ApplyTheme(_settings.LastTheme);

            var shellViewModel = new ShellViewModel(
                logger,
                fileWriter,
                navigationService,
                dialogService,
                themeService,
                toolWindowService,
                _settings);

            toolWindowService.Register<LogViewerViewModel>(vm => new LogViewerWindow { DataContext = vm });

            var window = new MainWindow { DataContext = shellViewModel };
            if (_settings.HasSavedWindowBounds)
            {
                window.Width = _settings.WindowWidth;
                window.Height = _settings.WindowHeight;
                window.Left = _settings.WindowLeft;
                window.Top = _settings.WindowTop;
            }

            window.Closing += (s, args) =>
            {
                var sourceWindow = window.WindowState == WindowState.Normal ? window : null;
                if (sourceWindow != null)
                {
                    _settings.WindowWidth = sourceWindow.Width;
                    _settings.WindowHeight = sourceWindow.Height;
                    _settings.WindowLeft = sourceWindow.Left;
                    _settings.WindowTop = sourceWindow.Top;
                    _settings.HasSavedWindowBounds = true;
                    _settings.Save();
                }
            };

            MainWindow = window;
            window.Show();

            navigationService.NavigateTo(_settings.LastSelectedNavKey);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var handler = AppServices.Resolve<IExceptionHandler>();
            handler.Handle(e.Exception, "UI dispatcher");
            e.Handled = true;
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var handler = AppServices.Resolve<IExceptionHandler>();
            var ex = e.ExceptionObject as Exception ?? new Exception("Unknown unhandled exception.");
            handler.Handle(ex, "AppDomain", e.IsTerminating);
        }
    }
}

