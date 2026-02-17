using System;

namespace AutomationShell48.Core.MVVM
{
    public abstract class DialogViewModelBase : BaseViewModel
    {
        private bool _canCloseOnEscape = true;
        private Action<bool?> _closeAction;

        public bool CanCloseOnEscape
        {
            get => _canCloseOnEscape;
            set => SetProperty(ref _canCloseOnEscape, value);
        }

        public void SetCloseAction(Action<bool?> closeAction)
        {
            _closeAction = closeAction;
        }

        protected void Close(bool? result)
        {
            _closeAction?.Invoke(result);
        }
    }
}
