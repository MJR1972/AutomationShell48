using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;
using AutomationShell48.Core.Dialogs;

namespace AutomationShell48.UI.Features.Projects
{
    public class ProjectsViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private readonly RelayCommand _simulateWorkCommand;

        public ProjectsViewModel(IDialogService dialogService, ILogger logger)
        {
            _dialogService = dialogService;
            _logger = logger;
            Title = "Projects";
            _simulateWorkCommand = new RelayCommand(async () => await SimulateWorkAsync(), () => !IsBusy);
        }

        public RelayCommand SimulateWorkCommand => _simulateWorkCommand;

        private async Task SimulateWorkAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Running project automation...";
                _simulateWorkCommand.RaiseCanExecuteChanged();

                var tcs = new TaskCompletionSource<bool>();
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    tcs.TrySetResult(true);
                };

                timer.Start();
                await tcs.Task.ConfigureAwait(true);

                IsBusy = false;
                BusyMessage = string.Empty;
                _simulateWorkCommand.RaiseCanExecuteChanged();

                _logger?.Info("Projects simulation completed.");
                await _dialogService.ShowDialogAsync(new MessageDialogViewModel("Done", "Project work finished successfully."));
            }
            catch (Exception ex)
            {
                IsBusy = false;
                BusyMessage = string.Empty;
                _simulateWorkCommand.RaiseCanExecuteChanged();
                _logger?.Error("SimulateWork failed.", ex);
            }
        }
    }
}


