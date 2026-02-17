using AutomationShell48.Core.MVVM;

namespace AutomationShell48.Core.ViewModels.Dialogs
{
    public class ConfirmDialogViewModel : DialogViewModelBase
    {
        private string _message;

        public ConfirmDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
            OkCommand = new RelayCommand(() => Close(true));
            CancelCommand = new RelayCommand(() => Close(false));
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public RelayCommand OkCommand { get; }

        public RelayCommand CancelCommand { get; }
    }
}
