using System;
using System.Threading.Tasks;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;

namespace AutomationShell48.Infrastructure.Services
{
    public class DialogService : IDialogService
    {
        private readonly object _sync = new object();
        private readonly ILogger _logger;
        private TaskCompletionSource<bool?> _activeDialog;

        public DialogService(ILogger logger)
        {
            _logger = logger;
        }

        public event Action<DialogViewModelBase> DialogOpened;
        public event Action DialogClosed;

        public Task<bool?> ShowDialogAsync(DialogViewModelBase viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            lock (_sync)
            {
                if (_activeDialog != null)
                {
                    _logger?.Warn("Dialog ignored because another dialog is already open.");
                    return Task.FromResult<bool?>(false);
                }

                _activeDialog = new TaskCompletionSource<bool?>();
                viewModel.SetCloseAction(CloseDialog);
                DialogOpened?.Invoke(viewModel);
                return _activeDialog.Task;
            }
        }

        public void CloseDialog(bool? result = null)
        {
            TaskCompletionSource<bool?> tcs = null;

            lock (_sync)
            {
                if (_activeDialog == null)
                {
                    return;
                }

                tcs = _activeDialog;
                _activeDialog = null;
            }

            try
            {
                tcs.TrySetResult(result);
            }
            finally
            {
                DialogClosed?.Invoke();
            }
        }
    }
}
