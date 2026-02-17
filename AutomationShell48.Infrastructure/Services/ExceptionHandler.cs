using System;
using System.Windows;
using AutomationShell48.Core.Services;
using AutomationShell48.Core.ViewModels.Dialogs;

namespace AutomationShell48.Infrastructure.Services
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;

        public ExceptionHandler(ILogger logger, IDialogService dialogService)
        {
            _logger = logger;
            _dialogService = dialogService;
        }

        public void Handle(Exception ex, string context = null, bool isTerminating = false)
        {
            var message = "Unexpected error" + (string.IsNullOrWhiteSpace(context) ? string.Empty : " (" + context + ")");
            _logger?.Error(message, ex);

            var dialogMessage = "Something went wrong. The error was logged. You can continue using the app.";
            if (isTerminating)
            {
                dialogMessage = "A fatal error occurred. The error was logged.";
            }

            try
            {
                var vm = new MessageDialogViewModel("Application Error", dialogMessage);
                _dialogService?.ShowDialogAsync(vm);
            }
            catch
            {
                try
                {
                    MessageBox.Show(dialogMessage, "AutomationShell48", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch
                {
                }
            }
        }
    }
}
