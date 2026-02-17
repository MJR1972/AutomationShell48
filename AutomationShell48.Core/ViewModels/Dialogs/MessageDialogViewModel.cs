using AutomationShell48.Core.MVVM;

namespace AutomationShell48.Core.ViewModels.Dialogs
{
    public class MessageDialogViewModel : DialogViewModelBase
    {
        private string _message;

        public MessageDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
            OkCommand = new RelayCommand(() => Close(true));
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public RelayCommand OkCommand { get; }
    }
}
